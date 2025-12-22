namespace Processing.Models;

/// <summary>
/// Biorhythm cycle information for a person on a specific date.
/// Based on the theory that human performance follows cyclic patterns.
/// </summary>
public class BiorhythmScore
{
    /// <summary>
    /// Physical cycle score (-1.0 to 1.0). Cycle length: 23 days.
    /// Affects strength, coordination, overall physical performance.
    /// </summary>
    public double Physical { get; set; }

    /// <summary>
    /// Emotional cycle score (-1.0 to 1.0). Cycle length: 28 days.
    /// Affects mood, creativity, emotional stability.
    /// </summary>
    public double Emotional { get; set; }

    /// <summary>
    /// Intellectual cycle score (-1.0 to 1.0). Cycle length: 33 days.
    /// Affects decision-making, memory, analytical thinking.
    /// </summary>
    public double Intellectual { get; set; }

    /// <summary>
    /// Combined weighted average of all biorhythm cycles.
    /// Default weights: Physical 50%, Emotional 25%, Intellectual 25%.
    /// </summary>
    public double Combined { get; set; }
}
