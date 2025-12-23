using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataRetrieval.Entities;

/// <summary>
/// Represents statistical performance data for a college football team.
/// </summary>
public class TeamStats
{
    /// <summary>
    /// Unique identifier for the stats record.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Team these statistics belong to.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TeamId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the team.
    /// </summary>
    [ForeignKey(nameof(TeamId))]
    public Team? Team { get; set; }

    /// <summary>
    /// Season year for these statistics (e.g., 2025).
    /// </summary>
    [Required]
    public int Season { get; set; }

    /// <summary>
    /// Points per game (offense).
    /// </summary>
    [Range(0, double.MaxValue)]
    public double Ppg { get; set; }

    /// <summary>
    /// Total offense national rank.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int TotalOffenseRank { get; set; }

    /// <summary>
    /// Passing yards national rank.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PassingYardsRank { get; set; }

    /// <summary>
    /// Rushing yards national rank.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int RushingYardsRank { get; set; }

    /// <summary>
    /// Total turnovers lost.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int TurnoversLost { get; set; }

    /// <summary>
    /// Points per game allowed (defense).
    /// </summary>
    [Range(0, double.MaxValue)]
    public double PpgAllowed { get; set; }

    /// <summary>
    /// Total defense national rank.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int TotalDefenseRank { get; set; }

    /// <summary>
    /// Passing yards allowed national rank.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PassingYardsAllowedRank { get; set; }

    /// <summary>
    /// Rushing yards allowed national rank.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int RushingYardsAllowedRank { get; set; }

    /// <summary>
    /// Total turnovers forced.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int TurnoversForced { get; set; }

    /// <summary>
    /// Turnover margin (forced - lost).
    /// </summary>
    public double TurnoverMargin { get; set; }
}
