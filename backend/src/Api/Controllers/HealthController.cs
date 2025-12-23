using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Health check endpoint to verify API is running and responsive.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Returns the health status of the API.
    /// </summary>
    /// <returns>Health status object</returns>
    [HttpGet]
    public IActionResult GetHealth()
    {
        _logger.LogInformation("Health check requested");
        
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}
