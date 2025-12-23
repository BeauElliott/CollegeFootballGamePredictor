using Microsoft.AspNetCore.Mvc;
using Processing.Models;
using Processing.Services;

namespace Api.Controllers;

/// <summary>
/// API controller for managing prediction model configuration.
/// Allows administrators to view and update weights, parameters, and position importance.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly ConfigService _configService;
    private readonly ILogger<ConfigController> _logger;

    public ConfigController(ConfigService configService, ILogger<ConfigController> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the currently active prediction configuration.
    /// </summary>
    /// <returns>Active configuration with all weights and parameters.</returns>
    /// <response code="200">Returns the active configuration.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(PredictionConfiguration), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PredictionConfiguration>> GetActiveConfiguration()
    {
        try
        {
            _logger.LogInformation("Getting active configuration");
            var config = await _configService.GetActiveConfigurationAsync();
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active configuration");
            return StatusCode(500, new { error = "Failed to retrieve configuration" });
        }
    }

    /// <summary>
    /// Gets all stored configurations.
    /// </summary>
    /// <returns>List of all configuration summaries.</returns>
    /// <response code="200">Returns all configurations.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ConfigurationSummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ConfigurationSummary>>> GetAllConfigurations()
    {
        try
        {
            _logger.LogInformation("Getting all configurations");
            var configs = await _configService.GetAllConfigurationsAsync();
            return Ok(configs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configurations");
            return StatusCode(500, new { error = "Failed to retrieve configurations" });
        }
    }

    /// <summary>
    /// Gets a specific configuration by ID.
    /// </summary>
    /// <param name="id">Configuration identifier.</param>
    /// <returns>Configuration details.</returns>
    /// <response code="200">Returns the requested configuration.</response>
    /// <response code="404">Configuration not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PredictionConfiguration), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PredictionConfiguration>> GetConfigurationById(int id)
    {
        try
        {
            _logger.LogInformation("Getting configuration {ConfigId}", id);
            var config = await _configService.GetConfigurationByIdAsync(id);
            return Ok(config);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Configuration {ConfigId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration {ConfigId}", id);
            return StatusCode(500, new { error = "Failed to retrieve configuration" });
        }
    }

    /// <summary>
    /// Updates the active prediction configuration.
    /// Creates a new configuration version and marks it as active.
    /// </summary>
    /// <param name="config">New configuration values.</param>
    /// <returns>Updated configuration summary.</returns>
    /// <response code="200">Configuration updated successfully.</response>
    /// <response code="400">Invalid configuration values.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("active")]
    [ProducesResponseType(typeof(ConfigurationSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ConfigurationSummary>> UpdateActiveConfiguration(
        [FromBody] PredictionConfiguration config)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate weights
            if (config.StatsWeight < 0 || config.StatsWeight > 1)
            {
                return BadRequest(new { error = "StatsWeight must be between 0 and 1" });
            }

            if (config.BiorhythmWeight < 0 || config.BiorhythmWeight > 1)
            {
                return BadRequest(new { error = "BiorhythmWeight must be between 0 and 1" });
            }

            if (Math.Abs(config.StatsWeight + config.BiorhythmWeight - 1.0) > 0.001)
            {
                return BadRequest(new { 
                    error = "StatsWeight and BiorhythmWeight must sum to 1.0",
                    currentSum = config.StatsWeight + config.BiorhythmWeight 
                });
            }

            if (config.HomeFieldAdvantage < 0 || config.HomeFieldAdvantage > 10)
            {
                return BadRequest(new { error = "HomeFieldAdvantage must be between 0 and 10" });
            }

            _logger.LogInformation(
                "Updating active configuration: Stats={Stats}, Bio={Bio}, Home={Home}",
                config.StatsWeight, config.BiorhythmWeight, config.HomeFieldAdvantage);

            var updatedConfig = await _configService.UpdateActiveConfigurationAsync(
                config, 
                User?.Identity?.Name ?? "Anonymous");

            return Ok(updatedConfig);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid configuration values");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating active configuration");
            return StatusCode(500, new { error = "Failed to update configuration" });
        }
    }

    /// <summary>
    /// Activates a previously saved configuration.
    /// </summary>
    /// <param name="id">Configuration ID to activate.</param>
    /// <returns>Activated configuration summary.</returns>
    /// <response code="200">Configuration activated successfully.</response>
    /// <response code="404">Configuration not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(typeof(ConfigurationSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ConfigurationSummary>> ActivateConfiguration(int id)
    {
        try
        {
            _logger.LogInformation("Activating configuration {ConfigId}", id);
            var activatedConfig = await _configService.ActivateConfigurationAsync(id);
            return Ok(activatedConfig);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Configuration {ConfigId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating configuration {ConfigId}", id);
            return StatusCode(500, new { error = "Failed to activate configuration" });
        }
    }

    /// <summary>
    /// Deletes a configuration by ID.
    /// Cannot delete the currently active configuration.
    /// </summary>
    /// <param name="id">Configuration ID to delete.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Configuration deleted successfully.</response>
    /// <response code="400">Cannot delete active configuration.</response>
    /// <response code="404">Configuration not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteConfiguration(int id)
    {
        try
        {
            _logger.LogInformation("Deleting configuration {ConfigId}", id);
            await _configService.DeleteConfigurationAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Configuration {ConfigId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete configuration {ConfigId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting configuration {ConfigId}", id);
            return StatusCode(500, new { error = "Failed to delete configuration" });
        }
    }
}
