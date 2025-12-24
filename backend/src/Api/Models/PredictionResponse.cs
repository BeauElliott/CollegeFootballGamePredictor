namespace Api.Models;

/// <summary>
/// Response containing game prediction results.
/// </summary>
public class PredictionResponse
{
    /// <summary>
    /// Unique identifier of the game.
    /// </summary>
    public string GameId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the predicted winning team.
    /// </summary>
    public string PredictedWinner { get; set; } = string.Empty;

    /// <summary>
    /// Team ID of the predicted winner.
    /// </summary>
    public string PredictedWinnerId { get; set; } = string.Empty;

    /// <summary>
    /// Probability of the predicted winner winning (0.0 to 1.0).
    /// </summary>
    public double WinProbability { get; set; }

    /// <summary>
    /// Timestamp when the prediction was generated.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Predicted margin of victory in points.
    /// </summary>
    public double Margin { get; set; }

    /// <summary>
    /// Breakdown of prediction factors.
    /// </summary>
    public PredictionBreakdownResponse Breakdown { get; set; } = new();
}

/// <summary>
/// Detailed breakdown of prediction factors.
/// </summary>
public class PredictionBreakdownResponse
{
    /// <summary>
    /// Statistical advantage in points.
    /// </summary>
    public double StatsEdge { get; set; }

    /// <summary>
    /// Biorhythm advantage in points.
    /// </summary>
    public double BiorhythmEdge { get; set; }

    /// <summary>
    /// Home field advantage in points.
    /// </summary>
    public double HomeFieldAdvantage { get; set; }

    /// <summary>
    /// Human-readable explanation of the prediction.
    /// </summary>
    public string Explanation { get; set; } = string.Empty;
}
