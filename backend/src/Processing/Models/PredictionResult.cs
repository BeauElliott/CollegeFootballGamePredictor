namespace Processing.Models;

/// <summary>
/// Result of a game prediction including winner, probability, and detailed breakdown.
/// </summary>
public class PredictionResult
{
    /// <summary>
    /// ID of the game being predicted.
    /// </summary>
    public string GameId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the predicted winning team.
    /// </summary>
    public string PredictedWinnerId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the predicted winning team.
    /// </summary>
    public string PredictedWinnerName { get; set; } = string.Empty;

    /// <summary>
    /// Probability of the predicted winner winning (0.0 to 1.0).
    /// </summary>
    public double WinProbability { get; set; }

    /// <summary>
    /// Predicted margin of victory in points (positive for winner).
    /// </summary>
    public double Margin { get; set; }

    /// <summary>
    /// Statistical advantage score.
    /// </summary>
    public double StatsEdge { get; set; }

    /// <summary>
    /// Biorhythm advantage score.
    /// </summary>
    public double BiorhythmEdge { get; set; }

    /// <summary>
    /// Home field advantage adjustment in points.
    /// </summary>
    public double HomeFieldAdjustment { get; set; }

    /// <summary>
    /// Human-readable explanation of the prediction.
    /// </summary>
    public string Explanation { get; set; } = string.Empty;
}
