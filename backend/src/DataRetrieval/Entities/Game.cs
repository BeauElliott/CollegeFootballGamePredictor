using System.ComponentModel.DataAnnotations;

namespace DataRetrieval.Entities;

/// <summary>
/// Represents the status of a college football game.
/// </summary>
public enum GameStatus
{
    Scheduled,
    Completed,
    Canceled,
    Postponed
}

/// <summary>
/// Represents a college football game between two teams.
/// </summary>
public class Game
{
    /// <summary>
    /// Unique identifier for the game.
    /// </summary>
    [Key]
    [Required]
    [MaxLength(100)]
    public string GameId { get; set; } = string.Empty;

    /// <summary>
    /// Date and time of the game.
    /// </summary>
    [Required]
    public DateTime Date { get; set; }

    /// <summary>
    /// Location/venue where the game is played.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the game.
    /// </summary>
    [Required]
    public GameStatus Status { get; set; } = GameStatus.Scheduled;

    /// <summary>
    /// ID of the home team.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string HomeTeamId { get; set; } = string.Empty;

    /// <summary>
    /// Home team navigation property.
    /// </summary>
    public Team? HomeTeam { get; set; }

    /// <summary>
    /// ID of the away team.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string AwayTeamId { get; set; } = string.Empty;

    /// <summary>
    /// Away team navigation property.
    /// </summary>
    public Team? AwayTeam { get; set; }

    /// <summary>
    /// Final score for home team (null if not completed).
    /// </summary>
    public int? HomeScore { get; set; }

    /// <summary>
    /// Final score for away team (null if not completed).
    /// </summary>
    public int? AwayScore { get; set; }
}
