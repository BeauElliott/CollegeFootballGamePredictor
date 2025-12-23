namespace Processing.Models;

/// <summary>
/// Configuration settings for the prediction model weights and parameters.
/// </summary>
public class PredictionConfiguration
{
    /// <summary>
    /// Weight given to traditional statistical analysis (0.0 to 1.0).
    /// Default: 0.8 (80%).
    /// </summary>
    public double StatsWeight { get; set; } = 0.8;

    /// <summary>
    /// Weight given to biorhythm analysis (0.0 to 1.0).
    /// Default: 0.2 (20%).
    /// </summary>
    public double BiorhythmWeight { get; set; } = 0.2;

    /// <summary>
    /// Home field advantage in points.
    /// Default: 3.0 points.
    /// </summary>
    public double HomeFieldAdvantage { get; set; } = 3.0;

    /// <summary>
    /// Position importance weights for biorhythm calculations.
    /// Higher values mean the position has more impact on the game.
    /// </summary>
    public Dictionary<string, double> PositionImportance { get; set; } = new()
    {
        { "QB", 1.0 },    // Quarterback - highest impact
        { "RB", 0.7 },    // Running back
        { "WR", 0.6 },    // Wide receiver
        { "TE", 0.5 },    // Tight end
        { "OL", 0.6 },    // Offensive line
        { "DL", 0.7 },    // Defensive line
        { "LB", 0.7 },    // Linebacker
        { "DB", 0.6 },    // Defensive back
        { "K", 0.3 },     // Kicker
        { "P", 0.2 }      // Punter
    };
}
