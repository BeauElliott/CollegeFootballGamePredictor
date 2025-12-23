using System.ComponentModel.DataAnnotations;

namespace DataRetrieval.Entities;

/// <summary>
/// Represents a college football team.
/// </summary>
public class Team
{
    /// <summary>
    /// Unique identifier for the team.
    /// </summary>
    [Key]
    [Required]
    [MaxLength(100)]
    public string TeamId { get; set; } = string.Empty;

    /// <summary>
    /// Official name of the team.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Conference affiliation (e.g., SEC, Big Ten, ACC, Big 12).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Conference { get; set; } = string.Empty;

    /// <summary>
    /// Team's current season statistics.
    /// </summary>
    public TeamStats? Stats { get; set; }

    /// <summary>
    /// Players on the team roster.
    /// </summary>
    public ICollection<Player> Roster { get; set; } = new List<Player>();

    /// <summary>
    /// Home games for this team.
    /// </summary>
    public ICollection<Game> HomeGames { get; set; } = new List<Game>();

    /// <summary>
    /// Away games for this team.
    /// </summary>
    public ICollection<Game> AwayGames { get; set; } = new List<Game>();
}
