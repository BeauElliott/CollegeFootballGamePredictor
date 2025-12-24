using DataRetrieval.Entities;

namespace Api.Models;

/// <summary>
/// Data transfer object for game information with team names included.
/// Used to avoid circular reference issues when serializing games with team data.
/// </summary>
public class GameDto
{
    public string GameId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public GameStatus Status { get; set; }
    public string HomeTeamId { get; set; } = string.Empty;
    public string HomeTeamName { get; set; } = string.Empty;
    public string AwayTeamId { get; set; } = string.Empty;
    public string AwayTeamName { get; set; } = string.Empty;
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
}