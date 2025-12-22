using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataRetrieval.Entities;

/// <summary>
/// Represents a game outcome prediction with probability and analysis breakdown.
/// </summary>
public class Prediction
{
    /// <summary>
    /// Unique identifier for the prediction.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Game this prediction is for.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string GameId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the game.
    /// </summary>
    [ForeignKey(nameof(GameId))]
    public Game? Game { get; set; }

    /// <summary>
    /// Team ID of the predicted winner.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string PredictedWinnerId { get; set; } = string.Empty;

    /// <summary>
    /// Probability of the predicted winner winning (0.0 to 1.0).
    /// </summary>
    [Required]
    [Range(0.0, 1.0)]
    public double WinProbability { get; set; }

    /// <summary>
    /// Predicted margin of victory (positive for predicted winner).
    /// </summary>
    public double Margin { get; set; }

    /// <summary>
    /// Detailed breakdown of the prediction factors.
    /// </summary>
    public PredictionBreakdown? Breakdown { get; set; }

    /// <summary>
    /// Timestamp when the prediction was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the prediction was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
