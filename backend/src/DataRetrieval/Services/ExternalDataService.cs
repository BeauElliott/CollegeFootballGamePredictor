using DataRetrieval.Configuration;
using DataRetrieval.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace DataRetrieval.Services;

/// <summary>
/// Service for retrieving college football data from external APIs.
/// Implements rate limiting and error handling for data source interactions.
/// </summary>
public class ExternalDataService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalDataService> _logger;
    private readonly DataSourcesConfiguration _config;

    public ExternalDataService(
        HttpClient httpClient,
        ILogger<ExternalDataService> logger,
        IOptions<DataSourcesConfiguration> config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config.Value;
    }

    /// <summary>
    /// Fetches games for a specific season and week from College Football Data API.
    /// </summary>
    /// <param name="season">Season year (e.g., 2025).</param>
    /// <param name="week">Week number (1-15 for regular season).</param>
    /// <returns>List of game data.</returns>
    public virtual async Task<List<ExternalGameResponse>> GetGamesAsync(int season, int? week = null)
    {
        if (!_config.CollegeFootballData.Enabled)
        {
            _logger.LogWarning("College Football Data API is disabled");
            return new List<ExternalGameResponse>();
        }

        try
        {
            var url = $"{_config.CollegeFootballData.BaseUrl}/games?year={season}";
            if (week.HasValue)
            {
                url += $"&week={week}";
            }

            _httpClient.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(_config.CollegeFootballData.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.CollegeFootballData.ApiKey}");
            }

            _logger.LogInformation("Fetching games for season {Season}, week {Week}", season, week);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var games = await response.Content.ReadFromJsonAsync<List<ExternalGameResponse>>();
            
            _logger.LogInformation("Retrieved {Count} games", games?.Count ?? 0);
            
            return games ?? new List<ExternalGameResponse>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching games from external API");
            throw new InvalidOperationException("Failed to fetch games from external data source", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching games");
            throw;
        }
    }

    /// <summary>
    /// Fetches team statistics for a specific season from College Football Data API.
    /// </summary>
    /// <param name="season">Season year (e.g., 2025).</param>
    /// <returns>List of team statistics.</returns>
    public virtual async Task<List<ExternalTeamStatsResponse>> GetTeamStatsAsync(int season)
    {
        if (!_config.CollegeFootballData.Enabled)
        {
            _logger.LogWarning("College Football Data API is disabled");
            return new List<ExternalTeamStatsResponse>();
        }

        try
        {
            var url = $"{_config.CollegeFootballData.BaseUrl}/stats/season/advanced?year={season}";

            _httpClient.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(_config.CollegeFootballData.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.CollegeFootballData.ApiKey}");
            }

            _logger.LogInformation("Fetching team stats for season {Season} from: {Url}", season, url);

            var response = await _httpClient.GetAsync(url);
            
            // Check for specific error codes that might indicate data unavailability
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Team stats data not available for season {Season} (404 Not Found)", season);
                return new List<ExternalTeamStatsResponse>();
            }
            
            response.EnsureSuccessStatusCode();

            var stats = await response.Content.ReadFromJsonAsync<List<ExternalTeamStatsResponse>>();
            
            _logger.LogInformation("Retrieved stats for {Count} teams", stats?.Count ?? 0);
            
            return stats ?? new List<ExternalTeamStatsResponse>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching team stats from external API");
            throw new InvalidOperationException("Failed to fetch team stats from external data source", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching team stats");
            throw;
        }
    }

    /// <summary>
    /// Fetches roster information for a specific team from College Football Data API.
    /// </summary>
    /// <param name="teamName">Team name.</param>
    /// <param name="season">Season year (e.g., 2025).</param>
    /// <returns>List of players on the team roster.</returns>
    public virtual async Task<List<ExternalPlayerResponse>> GetTeamRosterAsync(string teamName, int season)
    {
        if (!_config.CollegeFootballData.Enabled)
        {
            _logger.LogWarning("College Football Data API is disabled");
            return new List<ExternalPlayerResponse>();
        }

        try
        {
            var url = $"{_config.CollegeFootballData.BaseUrl}/roster?team={Uri.EscapeDataString(teamName)}&year={season}";

            _httpClient.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(_config.CollegeFootballData.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.CollegeFootballData.ApiKey}");
            }

            _logger.LogInformation("Fetching roster for team {Team}, season {Season} from: {Url}", teamName, season, url);

            var response = await _httpClient.GetAsync(url);
            
            // Check for specific error codes that might indicate data unavailability
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Roster data not available for team {Team}, season {Season} (404 Not Found)", teamName, season);
                return new List<ExternalPlayerResponse>();
            }
            
            response.EnsureSuccessStatusCode();

            var roster = await response.Content.ReadFromJsonAsync<List<ExternalPlayerResponse>>();
            
            _logger.LogInformation("Retrieved {Count} players for {Team}", roster?.Count ?? 0, teamName);
            
            return roster ?? new List<ExternalPlayerResponse>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching roster for {Team}", teamName);
            throw new InvalidOperationException($"Failed to fetch roster for {teamName} from external data source", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching roster for {Team}", teamName);
            throw;
        }
    }

    /// <summary>
    /// Fetches player usage statistics for a specific team and season.
    /// </summary>
    /// <param name="teamName">Team name.</param>
    /// <param name="season">Season year (e.g., 2025).</param>
    /// <returns>List of player usage statistics.</returns>
    public virtual async Task<List<ExternalPlayerUsageResponse>> GetPlayerUsageAsync(string teamName, int season)
    {
        if (!_config.CollegeFootballData.Enabled)
        {
            _logger.LogWarning("College Football Data API is disabled");
            return new List<ExternalPlayerUsageResponse>();
        }

        try
        {
            var url = $"{_config.CollegeFootballData.BaseUrl}/player/usage?team={Uri.EscapeDataString(teamName)}&year={season}";

            _httpClient.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(_config.CollegeFootballData.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.CollegeFootballData.ApiKey}");
            }

            _logger.LogInformation("Fetching player usage for team {Team}, season {Season}", teamName, season);

            var response = await _httpClient.GetAsync(url);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Player usage data not available for team {Team}, season {Season} (404 Not Found)", teamName, season);
                return new List<ExternalPlayerUsageResponse>();
            }
            
            response.EnsureSuccessStatusCode();

            var usage = await response.Content.ReadFromJsonAsync<List<ExternalPlayerUsageResponse>>();
            
            _logger.LogInformation("Retrieved usage data for {Count} players for {Team}", usage?.Count ?? 0, teamName);
            
            return usage ?? new List<ExternalPlayerUsageResponse>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching player usage for {Team}", teamName);
            // Don't throw - this is supplementary data
            return new List<ExternalPlayerUsageResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching player usage for {Team}", teamName);
            return new List<ExternalPlayerUsageResponse>();
        }
    }

    /// <summary>
    /// Fetches list of all teams from College Football Data API.
    /// </summary>
    /// <returns>List of teams.</returns>
    public virtual async Task<List<ExternalTeamResponse>> GetTeamsAsync()
    {
        if (!_config.CollegeFootballData.Enabled)
        {
            _logger.LogWarning("College Football Data API is disabled");
            return new List<ExternalTeamResponse>();
        }

        try
        {
            var url = $"{_config.CollegeFootballData.BaseUrl}/teams/fbs";

            _httpClient.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(_config.CollegeFootballData.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.CollegeFootballData.ApiKey}");
            }

            _logger.LogInformation("Fetching list of FBS teams");

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var teams = await response.Content.ReadFromJsonAsync<List<ExternalTeamResponse>>();
            
            _logger.LogInformation("Retrieved {Count} teams", teams?.Count ?? 0);
            
            return teams ?? new List<ExternalTeamResponse>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching teams from external API");
            throw new InvalidOperationException("Failed to fetch teams from external data source", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching teams");
            throw;
        }
    }
}
