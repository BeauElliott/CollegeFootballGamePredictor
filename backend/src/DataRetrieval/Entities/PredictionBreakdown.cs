using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataRetrieval.Entities;

/// <summary>
/// Detailed breakdown of prediction factors showing how each component contributed to the final prediction.
/// </summary>
public class PredictionBreakdown
{
    /// <summary>
    /// Unique identifier for the breakdown.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Prediction this breakdown belongs to.
    /// </summary>
    [Required]
    public int PredictionId { get; set; }

    /// <summary>
    /// Navigation property to the prediction.
    /// </summary>
    [ForeignKey(nameof(PredictionId))]
    public Prediction? Prediction { get; set; }

    /// <summary>
    /// Advantage from traditional statistics (offensive/defensive rankings, etc.).
    /// Positive value favors predicted winner.
    /// </summary>
    public double StatsEdge { get; set; }

    /// <summary>
    /// Advantage from biorhythm calculations for key players.
    /// Positive value favors predicted winner.
    /// </summary>
    public double BiorhythmEdge { get; set; }

    /// <summary>
    /// Home field advantage adjustment (typically 3-7 points).
    /// Positive if predicted winner is home team.
    /// </summary>
    public double HomeFieldAdjustment { get; set; }

    /// <summary>
    /// Human-readable explanation of the prediction factors and reasoning.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Explanation { get; set; } = string.Empty;
}
