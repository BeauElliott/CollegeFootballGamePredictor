using DataRetrieval.Data;
using DataRetrieval.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

/// <summary>
/// API controller for accessing core football data.
/// Provides read-only access to schedule, teams, statistics, and roster information.
/// </summary>
[ApiController]
[Route("api")]
public class CoreDataController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CoreDataController> _logger;

    public CoreDataController(ApplicationDbContext context, ILogger<CoreDataController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets upcoming games that haven't been played yet.
    /// Returns games ordered by date.
    /// </summary>
    /// <param name="limit">Maximum number of games to return (default: 20).</param>
    /// <returns>List of upcoming games.</returns>
    /// <response code="200">Returns list of upcoming games.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("schedule/upcoming")]
    [ProducesResponseType(typeof(List<Game>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<Game>>> GetUpcomingGames([FromQuery] int limit = 20)
    {
        try
        {
            _logger.LogInformation("Getting upcoming games, limit: {Limit}", limit);

            var games = await _context.Games
                .Where(g => g.Status == GameStatus.Scheduled && g.Date > DateTime.UtcNow)
                .OrderBy(g => g.Date)
                .Include(g => g.HomeTeam)
                .Include(g => g.AwayTeam)
                .Take(limit)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} upcoming games", games.Count);
            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upcoming games");
            return StatusCode(500, new { error = "Failed to retrieve upcoming games" });
        }
    }

    /// <summary>
    /// Gets a specific game by ID with full details.
    /// </summary>
    /// <param name="gameId">Game identifier.</param>
    /// <returns>Game details including teams.</returns>
    /// <response code="200">Returns the game.</response>
    /// <response code="404">Game not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("schedule/{gameId}")]
    [ProducesResponseType(typeof(Game), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Game>> GetGameById(string gameId)
    {
        try
        {
            _logger.LogInformation("Getting game {GameId}", gameId);

            var game = await _context.Games
                .Include(g => g.HomeTeam)
                .Include(g => g.AwayTeam)
                .FirstOrDefaultAsync(g => g.GameId == gameId);

            if (game == null)
            {
                _logger.LogWarning("Game {GameId} not found", gameId);
                return NotFound(new { error = $"Game {gameId} not found" });
            }

            return Ok(game);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving game {GameId}", gameId);
            return StatusCode(500, new { error = "Failed to retrieve game" });
        }
    }

    /// <summary>
    /// Gets all teams with basic information.
    /// </summary>
    /// <param name="conference">Optional conference filter (e.g., "SEC", "Big Ten").</param>
    /// <returns>List of teams.</returns>
    /// <response code="200">Returns list of teams.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("teams")]
    [ProducesResponseType(typeof(List<Team>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<Team>>> GetTeams([FromQuery] string? conference = null)
    {
        try
        {
            _logger.LogInformation("Getting teams, conference: {Conference}", conference ?? "all");

            var query = _context.Teams.AsQueryable();

            if (!string.IsNullOrEmpty(conference))
            {
                query = query.Where(t => t.Conference == conference);
            }

            var teams = await query
                .OrderBy(t => t.Conference)
                .ThenBy(t => t.Name)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} teams", teams.Count);
            return Ok(teams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teams");
            return StatusCode(500, new { error = "Failed to retrieve teams" });
        }
    }

    /// <summary>
    /// Gets statistics for a specific team.
    /// Returns the most recent stats if not specified.
    /// </summary>
    /// <param name="teamId">Team identifier.</param>
    /// <returns>Team statistics.</returns>
    /// <response code="200">Returns team statistics.</response>
    /// <response code="404">Team or stats not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("teams/{teamId}/stats")]
    [ProducesResponseType(typeof(TeamStats), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TeamStats>> GetTeamStats(string teamId)
    {
        try
        {
            _logger.LogInformation("Getting stats for team {TeamId}", teamId);

            // Verify team exists
            var team = await _context.Teams
                .Include(t => t.Stats)
                .FirstOrDefaultAsync(t => t.TeamId == teamId);

            if (team == null)
            {
                _logger.LogWarning("Team {TeamId} not found", teamId);
                return NotFound(new { error = $"Team {teamId} not found" });
            }

            if (team.Stats == null)
            {
                _logger.LogWarning("No stats found for team {TeamId}", teamId);
                return NotFound(new { error = $"No statistics found for team {teamId}" });
            }

            return Ok(team.Stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stats for team {TeamId}", teamId);
            return StatusCode(500, new { error = "Failed to retrieve team statistics" });
        }
    }

    /// <summary>
    /// Gets roster information for a specific team.
    /// Returns all players on the team, optionally filtered by position.
    /// </summary>
    /// <param name="teamId">Team identifier.</param>
    /// <param name="position">Optional position filter (e.g., "QB", "RB").</param>
    /// <returns>List of players on the roster.</returns>
    /// <response code="200">Returns team roster.</response>
    /// <response code="404">Team not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("teams/{teamId}/roster")]
    [ProducesResponseType(typeof(List<Player>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<Player>>> GetTeamRoster(
        string teamId,
        [FromQuery] string? position = null)
    {
        try
        {
            _logger.LogInformation(
                "Getting roster for team {TeamId}, position: {Position}",
                teamId, position);

            // Verify team exists
            var teamExists = await _context.Teams.AnyAsync(t => t.TeamId == teamId);
            if (!teamExists)
            {
                _logger.LogWarning("Team {TeamId} not found", teamId);
                return NotFound(new { error = $"Team {teamId} not found" });
            }

            var query = _context.Players
                .Where(p => p.TeamId == teamId);

            if (!string.IsNullOrEmpty(position))
            {
                query = query.Where(p => p.Position == position);
            }

            var roster = await query
                .OrderBy(p => p.Position)
                .ThenBy(p => p.Name)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} players for team {TeamId}", roster.Count, teamId);
            return Ok(roster);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roster for team {TeamId}", teamId);
            return StatusCode(500, new { error = "Failed to retrieve team roster" });
        }
    }

    /// <summary>
    /// Gets all games, optionally filtered by team or status.
    /// </summary>
    /// <param name="teamId">Optional team ID to filter by.</param>
    /// <param name="status">Optional game status filter.</param>
    /// <returns>List of games.</returns>
    /// <response code="200">Returns list of games.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("schedule")]
    [ProducesResponseType(typeof(List<Game>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<Game>>> GetSchedule(
        [FromQuery] string? teamId = null,
        [FromQuery] GameStatus? status = null)
    {
        try
        {
            _logger.LogInformation(
                "Getting schedule for team: {TeamId}, status: {Status}",
                teamId, status);

            var query = _context.Games
                .Include(g => g.HomeTeam)
                .Include(g => g.AwayTeam)
                .AsQueryable();

            if (!string.IsNullOrEmpty(teamId))
            {
                query = query.Where(g => g.HomeTeamId == teamId || g.AwayTeamId == teamId);
            }

            if (status.HasValue)
            {
                query = query.Where(g => g.Status == status.Value);
            }

            var games = await query
                .OrderBy(g => g.Date)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} games", games.Count);
            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedule");
            return StatusCode(500, new { error = "Failed to retrieve schedule" });
        }
    }

    /// <summary>
    /// Gets prediction history for a specific game.
    /// </summary>
    /// <param name="gameId">Game identifier.</param>
    /// <returns>List of predictions made for this game.</returns>
    /// <response code="200">Returns prediction history.</response>
    /// <response code="404">Game not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("schedule/{gameId}/predictions")]
    [ProducesResponseType(typeof(List<Prediction>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<Prediction>>> GetGamePredictions(string gameId)
    {
        try
        {
            _logger.LogInformation("Getting predictions for game {GameId}", gameId);

            var gameExists = await _context.Games.AnyAsync(g => g.GameId == gameId);
            if (!gameExists)
            {
                _logger.LogWarning("Game {GameId} not found", gameId);
                return NotFound(new { error = $"Game {gameId} not found" });
            }

            var predictions = await _context.Predictions
                .Where(p => p.GameId == gameId)
                .Include(p => p.Breakdown)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} predictions for game {GameId}", predictions.Count, gameId);
            return Ok(predictions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving predictions for game {GameId}", gameId);
            return StatusCode(500, new { error = "Failed to retrieve predictions" });
        }
    }
}
