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

                var homeTeamId = await GetTeamIdByNameAsync(extGame.HomeTeam);
                var awayTeamId = await GetTeamIdByNameAsync(extGame.AwayTeam);

                // Skip this game if we can't find the teams
                if (homeTeamId == null || awayTeamId == null)
                {
                    _logger.LogWarning("Skipping game {GameId}: Could not find team IDs for {HomeTeam} vs {AwayTeam}", 
                        gameId, extGame.HomeTeam, extGame.AwayTeam);
                    continue;
                }

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
    /// Refreshes team statistics for a specific season by calculating from game data.
    /// </summary>
    /// <param name="season">Season year (e.g., 2025).</param>
    /// <returns>Number of team stats records added or updated.</returns>
    public async Task<int> RefreshTeamStatsAsync(int season)
    {
        _logger.LogInformation("Starting team stats refresh for season {Season} (calculating from game data)", season);
        
        try
        {
            var externalStats = await _externalDataService.GetTeamStatsAsync(season);
            var teams = await _context.Teams.ToListAsync();
            int updatedCount = 0;

            // First pass: calculate all stats
            var teamStatsData = new List<(string TeamId, double Ppg, double PpgAllowed, double OffensePPA, double DefensePPA, double RushingPPA, double PassingPPA, double RushingPPAAllowed, double PassingPPAAllowed)>();
            
            foreach (var team in teams)
            {
                // Get all games for this team in the season
                var homeGames = await _context.Games
                    .Where(g => g.HomeTeamId == team.TeamId && g.Date.Year == season && g.Status == GameStatus.Completed)
                    .ToListAsync();
                
                var awayGames = await _context.Games
                    .Where(g => g.AwayTeamId == team.TeamId && g.Date.Year == season && g.Status == GameStatus.Completed)
                    .ToListAsync();

                var totalGames = homeGames.Count + awayGames.Count;
                if (totalGames == 0)
                {
                    continue;
                }

                // Calculate points scored and allowed
                var pointsScored = homeGames.Sum(g => g.HomeScore ?? 0) + awayGames.Sum(g => g.AwayScore ?? 0);
                var pointsAllowed = homeGames.Sum(g => g.AwayScore ?? 0) + awayGames.Sum(g => g.HomeScore ?? 0);

                var ppg = (double)pointsScored / totalGames;
                var ppgAllowed = (double)pointsAllowed / totalGames;

                // Get advanced stats for additional metrics
                var advancedStats = externalStats.FirstOrDefault(s => s.Team == team.Name);
                var offensePPA = advancedStats?.Offense?.Ppa ?? 0;
                var defensePPA = advancedStats?.Defense?.Ppa ?? 0;
                var rushingPPA = advancedStats?.Offense?.RushingPlays?.Ppa ?? 0;
                var passingPPA = advancedStats?.Offense?.PassingPlays?.Ppa ?? 0;
                var rushingPPAAllowed = advancedStats?.Defense?.RushingPlays?.Ppa ?? 0;
                var passingPPAAllowed = advancedStats?.Defense?.PassingPlays?.Ppa ?? 0;

                teamStatsData.Add((team.TeamId, ppg, ppgAllowed, offensePPA, defensePPA, rushingPPA, passingPPA, rushingPPAAllowed, passingPPAAllowed));
            }

            // Calculate rankings
            var ppgRanked = teamStatsData.OrderByDescending(t => t.Ppg).Select((t, i) => (t.TeamId, Rank: i + 1)).ToDictionary(x => x.TeamId, x => x.Rank);
            var ppgAllowedRanked = teamStatsData.OrderBy(t => t.PpgAllowed).Select((t, i) => (t.TeamId, Rank: i + 1)).ToDictionary(x => x.TeamId, x => x.Rank);
            var offensePPARanked = teamStatsData.OrderByDescending(t => t.OffensePPA).Select((t, i) => (t.TeamId, Rank: i + 1)).ToDictionary(x => x.TeamId, x => x.Rank);
            var defensePPARanked = teamStatsData.OrderBy(t => t.DefensePPA).Select((t, i) => (t.TeamId, Rank: i + 1)).ToDictionary(x => x.TeamId, x => x.Rank);
            var rushingPPARanked = teamStatsData.OrderByDescending(t => t.RushingPPA).Select((t, i) => (t.TeamId, Rank: i + 1)).ToDictionary(x => x.TeamId, x => x.Rank);
            var passingPPARanked = teamStatsData.OrderByDescending(t => t.PassingPPA).Select((t, i) => (t.TeamId, Rank: i + 1)).ToDictionary(x => x.TeamId, x => x.Rank);
            var rushingPPAAllowedRanked = teamStatsData.OrderBy(t => t.RushingPPAAllowed).Select((t, i) => (t.TeamId, Rank: i + 1)).ToDictionary(x => x.TeamId, x => x.Rank);
            var passingPPAAllowedRanked = teamStatsData.OrderBy(t => t.PassingPPAAllowed).Select((t, i) => (t.TeamId, Rank: i + 1)).ToDictionary(x => x.TeamId, x => x.Rank);

            // Second pass: save stats with rankings
            foreach (var teamData in teamStatsData)
            {
                var existingStats = await _context.TeamStats
                    .FirstOrDefaultAsync(s => s.TeamId == teamData.TeamId && s.Season == season);

                if (existingStats != null)
                {
                    // Update existing stats
                    existingStats.Ppg = teamData.Ppg;
                    existingStats.PpgAllowed = teamData.PpgAllowed;
                    existingStats.TotalOffenseRank = offensePPARanked.GetValueOrDefault(teamData.TeamId, 999);
                    existingStats.TotalDefenseRank = defensePPARanked.GetValueOrDefault(teamData.TeamId, 999);
                    existingStats.PassingYardsRank = passingPPARanked.GetValueOrDefault(teamData.TeamId, 999);
                    existingStats.RushingYardsRank = rushingPPARanked.GetValueOrDefault(teamData.TeamId, 999);
                    existingStats.PassingYardsAllowedRank = passingPPAAllowedRanked.GetValueOrDefault(teamData.TeamId, 999);
                    existingStats.RushingYardsAllowedRank = rushingPPAAllowedRanked.GetValueOrDefault(teamData.TeamId, 999);
                    existingStats.TurnoversLost = 0; // Not available in current APIs
                    existingStats.TurnoversForced = 0; // Not available in current APIs
                    existingStats.TurnoverMargin = 0; // Not available in current APIs
                    updatedCount++;
                }
                else
                {
                    // Add new stats
                    var newStats = new TeamStats
                    {
                        TeamId = teamData.TeamId,
                        Season = season,
                        Ppg = teamData.Ppg,
                        PpgAllowed = teamData.PpgAllowed,
                        TotalOffenseRank = offensePPARanked.GetValueOrDefault(teamData.TeamId, 999),
                        TotalDefenseRank = defensePPARanked.GetValueOrDefault(teamData.TeamId, 999),
                        PassingYardsRank = passingPPARanked.GetValueOrDefault(teamData.TeamId, 999),
                        RushingYardsRank = rushingPPARanked.GetValueOrDefault(teamData.TeamId, 999),
                        PassingYardsAllowedRank = passingPPAAllowedRanked.GetValueOrDefault(teamData.TeamId, 999),
                        RushingYardsAllowedRank = rushingPPAAllowedRanked.GetValueOrDefault(teamData.TeamId, 999),
                        TurnoversLost = 0,
                        TurnoversForced = 0,
                        TurnoverMargin = 0
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
            var playerUsage = await _externalDataService.GetPlayerUsageAsync(teamName, season);
            
            // Create a dictionary for quick usage lookup by player ID
            var usageByPlayerId = playerUsage.ToDictionary(u => u.Id, u => u.Usage?.Overall ?? 0);
            
            int updatedCount = 0;

            // Find the team by name, not by constructing team ID from name
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Name == teamName);
            
            if (team == null)
            {
                _logger.LogWarning("Team with name '{TeamName}' not found, cannot refresh roster", teamName);
                return 0;
            }

            var teamId = team.TeamId;

            // Estimate average plays per game (typical is ~65-75 plays per game)
            const double AVERAGE_PLAYS_PER_GAME = 70.0;

            foreach (var extPlayer in externalPlayers)
            {
                var playerId = $"player-{extPlayer.Id}";
                var existingPlayer = await _context.Players.FindAsync(playerId);

                // Get usage data for this player
                var usagePercentage = usageByPlayerId.GetValueOrDefault(extPlayer.Id.ToString(), 0);
                var snapsPerGame = usagePercentage * AVERAGE_PLAYS_PER_GAME;
                
                // Estimate depth chart based on snap percentage
                var depthChart = EstimateDepthChart(usagePercentage, extPlayer.Position);

                if (existingPlayer != null)
                {
                    // Update existing player
                    existingPlayer.Name = extPlayer.FullName;
                    existingPlayer.Position = extPlayer.Position ?? "Unknown";
                    existingPlayer.TeamId = teamId;
                    existingPlayer.SnapsPerGame = snapsPerGame;
                    existingPlayer.DepthChart = depthChart;
                    updatedCount++;
                }
                else
                {
                    // Add new player
                    var newPlayer = new Player
                    {
                        PlayerId = playerId,
                        Name = extPlayer.FullName,
                        Position = extPlayer.Position ?? "Unknown",
                        TeamId = teamId,
                        DateOfBirth = DateTime.UtcNow.AddYears(-20), // Placeholder
                        DepthChart = depthChart,
                        SnapsPerGame = snapsPerGame
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
    /// Estimates depth chart position based on snap percentage and position.
    /// </summary>
    private string EstimateDepthChart(double usagePercentage, string? position)
    {
        // Special handling for specialists who may have high usage but aren't "starters" in traditional sense
        if (position == "K" || position == "P" || position == "LS")
        {
            return usagePercentage > 0.5 ? "Starter" : "Backup";
        }

        // For other positions, use snap percentage thresholds
        if (usagePercentage >= 0.70)
        {
            return "Starter (1st)";
        }
        else if (usagePercentage >= 0.30)
        {
            return "Backup (2nd)";
        }
        else if (usagePercentage >= 0.10)
        {
            return "Reserve (3rd)";
        }
        else if (usagePercentage > 0)
        {
            return "Deep Reserve";
        }
        else
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Refreshes roster data for all teams.
    /// </summary>
    /// <param name="season">Season year (e.g., 2025).</param>
    /// <returns>Total number of players added or updated across all teams.</returns>
    public async Task<int> RefreshAllRostersAsync(int season)
    {
        _logger.LogInformation("Starting roster refresh for all teams, season {Season}", season);
        
        try
        {
            // Get all teams
            var teams = await _context.Teams.ToListAsync();
            int totalUpdated = 0;

            foreach (var team in teams)
            {
                try
                {
                    // Use the actual team name stored in the Name field
                    var playerCount = await RefreshTeamRosterAsync(team.Name, season);
                    totalUpdated += playerCount;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to refresh roster for team {TeamId}, continuing with other teams", team.TeamId);
                }
            }

            _logger.LogInformation("Roster refresh complete: {Count} total players processed across all teams", totalUpdated);
            return totalUpdated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during roster refresh for all teams");
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
        var errors = new List<string>();

        try
        {
            // 1. Refresh teams
            try
            {
                result.TeamsUpdated = await RefreshTeamsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh teams data");
                errors.Add($"Teams refresh failed: {ex.Message}");
            }

            // 2. Refresh schedule
            try
            {
                result.GamesUpdated = await RefreshScheduleAsync(season);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh schedule data");
                errors.Add($"Schedule refresh failed: {ex.Message}");
            }

            // 3. Refresh team stats
            try
            {
                result.StatsUpdated = await RefreshTeamStatsAsync(season);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh team stats data");
                errors.Add($"Team stats refresh failed: {ex.Message}");
            }

            // 4. Refresh rosters (optional, may be slow)
            try
            {
                result.RostersUpdated = await RefreshAllRostersAsync(season);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh roster data");
                errors.Add($"Roster refresh failed: {ex.Message}");
            }

            // Set success if we got at least teams or games, even if stats failed
            result.Success = result.TeamsUpdated > 0 || result.GamesUpdated > 0;
            result.CompletedAt = DateTime.UtcNow;

            if (errors.Any())
            {
                result.ErrorMessage = string.Join("; ", errors);
                _logger.LogWarning("Data refresh completed with errors: {Errors}", result.ErrorMessage);
            }
            else
            {
                _logger.LogInformation(
                    "Complete data refresh finished: {Teams} teams, {Games} games, {Stats} stats, {Rosters} rosters updated",
                    result.TeamsUpdated, result.GamesUpdated, result.StatsUpdated, result.RostersUpdated);
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error during complete data refresh");
        }

        return result;
    }

    /// <summary>
    /// Helper method to find team ID by team name.
    /// </summary>
    private async Task<string?> GetTeamIdByNameAsync(string teamName)
    {
        if (string.IsNullOrEmpty(teamName))
            return null;

        var team = await _context.Teams
            .FirstOrDefaultAsync(t => t.Name == teamName);

        return team?.TeamId;
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
