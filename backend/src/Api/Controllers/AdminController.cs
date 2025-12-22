using DataRetrieval.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Administrative endpoints for data management and system operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly DataRefreshService _dataRefreshService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        DataRefreshService dataRefreshService,
        ILogger<AdminController> logger)
    {
        _dataRefreshService = dataRefreshService;
        _logger = logger;
    }

    /// <summary>
    /// Refreshes all data (teams, schedule, and stats) for a specified season.
    /// </summary>
    /// <param name="season">Season year (e.g., 2025).</param>
    /// <returns>Summary of refresh operation results.</returns>
    /// <response code="200">Returns the refresh operation summary.</response>
    /// <response code="400">If the season is invalid.</response>
    /// <response code="500">If an error occurs during refresh.</response>
    [HttpPost("refresh/all")]
    [ProducesResponseType(typeof(DataRefreshResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DataRefreshResult>> RefreshAll([FromQuery] int season)
    {
        if (season < 2000 || season > DateTime.Now.Year + 1)
        {
            _logger.LogWarning("Invalid season requested: {Season}", season);
            return BadRequest(new { error = "Invalid season year" });
        }

        _logger.LogInformation("Admin triggered full data refresh for season {Season}", season);

        try
        {
            var result = await _dataRefreshService.RefreshAllAsync(season);
            
            if (!result.Success)
            {
                return StatusCode(500, result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during full data refresh");
            return StatusCode(500, new { error = "Data refresh failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Refreshes teams data from external sources.
    /// </summary>
    /// <returns>Number of teams updated.</returns>
    /// <response code="200">Returns the number of teams updated.</response>
    /// <response code="500">If an error occurs during refresh.</response>
    [HttpPost("refresh/teams")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> RefreshTeams()
    {
        _logger.LogInformation("Admin triggered teams refresh");

        try
        {
            var count = await _dataRefreshService.RefreshTeamsAsync();
            return Ok(new { teamsUpdated = count, message = $"Successfully updated {count} teams" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing teams");
            return StatusCode(500, new { error = "Teams refresh failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Refreshes schedule (games) data for a specific season and optional week.
    /// </summary>
    /// <param name="season">Season year (e.g., 2025).</param>
    /// <param name="week">Optional week number (1-15).</param>
    /// <returns>Number of games updated.</returns>
    /// <response code="200">Returns the number of games updated.</response>
    /// <response code="400">If parameters are invalid.</response>
    /// <response code="500">If an error occurs during refresh.</response>
    [HttpPost("refresh/schedule")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> RefreshSchedule([FromQuery] int season, [FromQuery] int? week = null)
    {
        if (season < 2000 || season > DateTime.Now.Year + 1)
        {
            return BadRequest(new { error = "Invalid season year" });
        }

        if (week.HasValue && (week < 1 || week > 15))
        {
            return BadRequest(new { error = "Week must be between 1 and 15" });
        }

        _logger.LogInformation("Admin triggered schedule refresh for season {Season}, week {Week}", season, week);

        try
        {
            var count = await _dataRefreshService.RefreshScheduleAsync(season, week);
            return Ok(new { gamesUpdated = count, message = $"Successfully updated {count} games" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing schedule");
            return StatusCode(500, new { error = "Schedule refresh failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Refreshes team statistics for a specific season.
    /// </summary>
    /// <param name="season">Season year (e.g., 2025).</param>
    /// <returns>Number of team stats records updated.</returns>
    /// <response code="200">Returns the number of stats records updated.</response>
    /// <response code="400">If the season is invalid.</response>
    /// <response code="500">If an error occurs during refresh.</response>
    [HttpPost("refresh/stats")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> RefreshStats([FromQuery] int season)
    {
        if (season < 2000 || season > DateTime.Now.Year + 1)
        {
            return BadRequest(new { error = "Invalid season year" });
        }

        _logger.LogInformation("Admin triggered stats refresh for season {Season}", season);

        try
        {
            var count = await _dataRefreshService.RefreshTeamStatsAsync(season);
            return Ok(new { statsUpdated = count, message = $"Successfully updated stats for {count} teams" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing stats");
            return StatusCode(500, new { error = "Stats refresh failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Refreshes roster data for a specific team.
    /// </summary>
    /// <param name="teamName">Team name.</param>
    /// <param name="season">Season year (e.g., 2025).</param>
    /// <returns>Number of players updated.</returns>
    /// <response code="200">Returns the number of players updated.</response>
    /// <response code="400">If parameters are invalid.</response>
    /// <response code="500">If an error occurs during refresh.</response>
    [HttpPost("refresh/roster")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> RefreshRoster([FromQuery] string teamName, [FromQuery] int season)
    {
        if (string.IsNullOrWhiteSpace(teamName))
        {
            return BadRequest(new { error = "Team name is required" });
        }

        if (season < 2000 || season > DateTime.Now.Year + 1)
        {
            return BadRequest(new { error = "Invalid season year" });
        }

        _logger.LogInformation("Admin triggered roster refresh for team {Team}, season {Season}", teamName, season);

        try
        {
            var count = await _dataRefreshService.RefreshTeamRosterAsync(teamName, season);
            return Ok(new { playersUpdated = count, message = $"Successfully updated roster with {count} players" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing roster for {Team}", teamName);
            return StatusCode(500, new { error = "Roster refresh failed", details = ex.Message });
        }
    }
}
