using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataRetrieval.Entities;

/// <summary>
/// Represents a college football player on a team roster.
/// </summary>
public class Player
{
    /// <summary>
    /// Unique identifier for the player.
    /// </summary>
    [Key]
    [Required]
    [MaxLength(100)]
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the player.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Playing position (e.g., QB, RB, WR, OL, DL, LB, DB, K, P).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Position { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth (used for biorhythm calculations).
    /// </summary>
    [Required]
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Depth chart designation (e.g., "Starter", "Backup", "2nd String").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string DepthChart { get; set; } = string.Empty;

    /// <summary>
    /// Average snaps per game (measure of playing time/importance).
    /// </summary>
    [Range(0, double.MaxValue)]
    public double SnapsPerGame { get; set; }

    /// <summary>
    /// Team this player belongs to.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TeamId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the team.
    /// </summary>
    [ForeignKey(nameof(TeamId))]
    public Team? Team { get; set; }
}
