using DataRetrieval.Data;
using DataRetrieval.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataRetrieval.Services;

/// <summary>
/// Service for refreshing and updating schedule, team stats, and roster data in the database.
/// Coordinates between external data sources and local storage.
/// </summary>
public class DataRefreshService
{
    private readonly ApplicationDbContext _context;
    private readonly ExternalDataService _externalDataService;
    private readonly ILogger<DataRefreshService> _logger;

    public DataRefreshService(
        ApplicationDbContext context,
        ExternalDataService externalDataService,
        ILogger<DataRefreshService> logger)
    {
        _context = context;
        _externalDataService = externalDataService;
        _logger = logger;
    }

    /// <summary>
    /// Refreshes teams data from external sources.
    /// </summary>
    /// <returns>Number of teams added or updated.</returns>
    public async Task<int> RefreshTeamsAsync()
    {
        _logger.LogInformation("Starting teams data refresh");
        
        try
        {
            var externalTeams = await _externalDataService.GetTeamsAsync();
            int updatedCount = 0;

            foreach (var extTeam in externalTeams)
            {
                var teamId = $"team-{extTeam.Id}";
                var existingTeam = await _context.Teams.FindAsync(teamId);

                if (existingTeam != null)
                {
                    // Update existing team
                    existingTeam.Name = extTeam.School;
                    existingTeam.Conference = extTeam.Conference ?? "Independent";
                    updatedCount++;
                }
                else
                {
                    // Add new team
                    var newTeam = new Team
                    {
                        TeamId = teamId,
                        Name = extTeam.School,
                        Conference = extTeam.Conference ?? "Independent"
                    };
                    _context.Teams.Add(newTeam);
                    updatedCount++;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Teams refresh complete: {Count} teams processed", updatedCount);
            
            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing teams data");
            throw;
        }
    }

    /// <summary>
    /// Refreshes schedule (games) data for a specific season.
    /// </summary>
    /// <param name="season">Season year (e.g., 2025).</param>
    /// <param name="week">Optional week number to refresh.</param>
    /// <returns>Number of games added or updated.</returns>
    public async Task<int> RefreshScheduleAsync(int season, int? week = null)
    {
        _logger.LogInformation("Starting schedule refresh for season {Season}, week {Week}", season, week);
        
        try
        {
            var externalGames = await _externalDataService.GetGamesAsync(season, week);
            int updatedCount = 0;

            foreach (var extGame in externalGames)
            {
                var gameId = $"game-{extGame.Id}";
                var existingGame = await _context.Games.FindAsync(gameId);

                var homeTeamId = $"team-{extGame.HomeTeam}";
                var awayTeamId = $"team-{extGame.AwayTeam}";

                if (existingGame != null)
                {
                    // Update existing game
                    existingGame.Date = extGame.StartDate;
                    existingGame.Location = extGame.Venue;
                    existingGame.HomeScore = extGame.HomePoints;
                    existingGame.AwayScore = extGame.AwayPoints;
                    existingGame.Status = extGame.Completed ? GameStatus.Completed : GameStatus.Scheduled;
                    updatedCount++;
                }
                else
                {
                    // Add new game
                    var newGame = new Game
                    {
                        GameId = gameId,
                        Date = extGame.StartDate,
                        Location = extGame.Venue,
                        HomeTeamId = homeTeamId,
                        AwayTeamId = awayTeamId,
                        HomeScore = extGame.HomePoints,
                        AwayScore = extGame.AwayPoints,
                        Status = extGame.Completed ? GameStatus.Completed : GameStatus.Scheduled
                    };
                    _context.Games.Add(newGame);
                    updatedCount++;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Schedule refresh complete: {Count} games processed", updatedCount);
            
            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing schedule data");
            throw;
        }
    }

    /// <summary>
    /// Refreshes team statistics for a specific season.
    /// </summary>
    /// <param name="season">Season year (e.g., 2025).</param>
    /// <returns>Number of team stats records added or updated.</returns>
    public async Task<int> RefreshTeamStatsAsync(int season)
    {
        _logger.LogInformation("Starting team stats refresh for season {Season}", season);
        
        try
        {
            var externalStats = await _externalDataService.GetTeamStatsAsync(season);
            int updatedCount = 0;

            foreach (var extStats in externalStats)
            {
                var teamId = $"team-{extStats.Team}";
                
                // Ensure team exists
                var team = await _context.Teams.FindAsync(teamId);
                if (team == null)
                {
                    _logger.LogWarning("Team {TeamId} not found, skipping stats", teamId);
                    continue;
                }

                var existingStats = await _context.TeamStats
                    .FirstOrDefaultAsync(s => s.TeamId == teamId && s.Season == season);

                if (existingStats != null)
                {
                    // Update existing stats
                    existingStats.Ppg = extStats.PointsPerGame ?? 0;
                    existingStats.PpgAllowed = extStats.PointsAllowed ?? 0;
                    existingStats.TotalOffenseRank = extStats.TotalOffenseRank ?? 999;
                    existingStats.TotalDefenseRank = extStats.TotalDefenseRank ?? 999;
                    existingStats.PassingYardsRank = extStats.PassingYardsRank ?? 999;
                    existingStats.RushingYardsRank = extStats.RushingYardsRank ?? 999;
                    existingStats.PassingYardsAllowedRank = extStats.PassingYardsAllowedRank ?? 999;
                    existingStats.RushingYardsAllowedRank = extStats.RushingYardsAllowedRank ?? 999;
                    existingStats.TurnoversLost = extStats.TurnoversLost ?? 0;
                    existingStats.TurnoversForced = extStats.TurnoversGained ?? 0;
                    existingStats.TurnoverMargin = extStats.TurnoverMargin ?? 0;
                    updatedCount++;
                }
                else
                {
                    // Add new stats
                    var newStats = new TeamStats
                    {
                        TeamId = teamId,
                        Season = season,
                        Ppg = extStats.PointsPerGame ?? 0,
                        PpgAllowed = extStats.PointsAllowed ?? 0,
                        TotalOffenseRank = extStats.TotalOffenseRank ?? 999,
                        TotalDefenseRank = extStats.TotalDefenseRank ?? 999,
                        PassingYardsRank = extStats.PassingYardsRank ?? 999,
                        RushingYardsRank = extStats.RushingYardsRank ?? 999,
                        PassingYardsAllowedRank = extStats.PassingYardsAllowedRank ?? 999,
                        RushingYardsAllowedRank = extStats.RushingYardsAllowedRank ?? 999,
                        TurnoversLost = extStats.TurnoversLost ?? 0,
                        TurnoversForced = extStats.TurnoversGained ?? 0,
                        TurnoverMargin = extStats.TurnoverMargin ?? 0
                    };
                    _context.TeamStats.Add(newStats);
                    updatedCount++;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Team stats refresh complete: {Count} records processed", updatedCount);
            
            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing team stats");
            throw;
        }
    }

    /// <summary>
    /// Refreshes roster data for a specific team.
    /// </summary>
    /// <param name="teamName">Team name.</param>
    /// <param name="season">Season year (e.g., 2025).</param>
    /// <returns>Number of players added or updated.</returns>
    public async Task<int> RefreshTeamRosterAsync(string teamName, int season)
    {
        _logger.LogInformation("Starting roster refresh for team {Team}, season {Season}", teamName, season);
        
        try
        {
            var externalPlayers = await _externalDataService.GetTeamRosterAsync(teamName, season);
            int updatedCount = 0;

            var teamId = $"team-{teamName}";
            var team = await _context.Teams.FindAsync(teamId);
            
            if (team == null)
            {
                _logger.LogWarning("Team {TeamId} not found, cannot refresh roster", teamId);
                return 0;
            }

            foreach (var extPlayer in externalPlayers)
            {
                var playerId = $"player-{extPlayer.Id}";
                var existingPlayer = await _context.Players.FindAsync(playerId);

                if (existingPlayer != null)
                {
                    // Update existing player
                    existingPlayer.Name = extPlayer.Name;
                    existingPlayer.Position = extPlayer.Position;
                    existingPlayer.TeamId = teamId;
                    // Note: DOB, snaps, and depth chart would need additional data sources
                    updatedCount++;
                }
                else
                {
                    // Add new player (with placeholder data for missing fields)
                    var newPlayer = new Player
                    {
                        PlayerId = playerId,
                        Name = extPlayer.Name,
                        Position = extPlayer.Position,
                        TeamId = teamId,
                        DateOfBirth = DateTime.Now.AddYears(-20), // Placeholder
                        DepthChart = "Unknown",
                        SnapsPerGame = 0
                    };
                    _context.Players.Add(newPlayer);
                    updatedCount++;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Roster refresh complete for {Team}: {Count} players processed", teamName, updatedCount);
            
            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing roster for team {Team}", teamName);
            throw;
        }
    }

    /// <summary>
    /// Performs a complete data refresh: teams, schedule, stats, and rosters.
    /// </summary>
    /// <param name="season">Season year (e.g., 2025).</param>
    /// <returns>Summary of refresh results.</returns>
    public async Task<DataRefreshResult> RefreshAllAsync(int season)
    {
        _logger.LogInformation("Starting complete data refresh for season {Season}", season);
        
        var result = new DataRefreshResult { Season = season };

        try
        {
            // 1. Refresh teams
            result.TeamsUpdated = await RefreshTeamsAsync();

            // 2. Refresh schedule
            result.GamesUpdated = await RefreshScheduleAsync(season);

            // 3. Refresh team stats
            result.StatsUpdated = await RefreshTeamStatsAsync(season);

            result.Success = true;
            result.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Complete data refresh finished: {Teams} teams, {Games} games, {Stats} stats records",
                result.TeamsUpdated, result.GamesUpdated, result.StatsUpdated);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error during complete data refresh");
        }

        return result;
    }
}

/// <summary>
/// Result of a data refresh operation.
/// </summary>
public class DataRefreshResult
{
    public bool Success { get; set; }
    public int Season { get; set; }
    public int TeamsUpdated { get; set; }
    public int GamesUpdated { get; set; }
    public int StatsUpdated { get; set; }
    public int RostersUpdated { get; set; }
    public DateTime CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
