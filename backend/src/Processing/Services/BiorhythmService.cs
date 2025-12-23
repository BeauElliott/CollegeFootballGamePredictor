using Processing.Models;

namespace Processing.Services;

/// <summary>
/// Service for calculating biorhythm scores based on date of birth.
/// Biorhythm theory suggests human performance follows cyclic patterns.
/// </summary>
public class BiorhythmService
{
    private const int PhysicalCycleLength = 23;
    private const int EmotionalCycleLength = 28;
    private const int IntellectualCycleLength = 33;

    /// <summary>
    /// Default weights for combining biorhythm cycles.
    /// Physical is weighted highest as it most directly affects athletic performance.
    /// </summary>
    private const double PhysicalWeight = 0.5;
    private const double EmotionalWeight = 0.25;
    private const double IntellectualWeight = 0.25;

    /// <summary>
    /// Calculates biorhythm scores for a person on a specific date.
    /// </summary>
    /// <param name="dateOfBirth">The person's date of birth.</param>
    /// <param name="targetDate">The date to calculate biorhythms for.</param>
    /// <returns>Biorhythm scores for physical, emotional, and intellectual cycles.</returns>
    public BiorhythmScore CalculateBiorhythm(DateTime dateOfBirth, DateTime targetDate)
    {
        // Calculate days since birth
        var daysSinceBirth = (targetDate.Date - dateOfBirth.Date).TotalDays;

        // Calculate each cycle using sine wave formula
        // Score ranges from -1.0 to 1.0, where 1.0 is peak performance
        var physical = Math.Sin(2 * Math.PI * daysSinceBirth / PhysicalCycleLength);
        var emotional = Math.Sin(2 * Math.PI * daysSinceBirth / EmotionalCycleLength);
        var intellectual = Math.Sin(2 * Math.PI * daysSinceBirth / IntellectualCycleLength);

        // Calculate weighted combined score
        var combined = (physical * PhysicalWeight) +
                       (emotional * EmotionalWeight) +
                       (intellectual * IntellectualWeight);

        return new BiorhythmScore
        {
            Physical = physical,
            Emotional = emotional,
            Intellectual = intellectual,
            Combined = combined
        };
    }

    /// <summary>
    /// Calculates aggregate biorhythm score for a group of players weighted by their importance.
    /// </summary>
    /// <param name="playerBiorhythms">Dictionary of player biorhythm scores.</param>
    /// <param name="playerWeights">Dictionary of player importance weights.</param>
    /// <returns>Weighted average biorhythm score for the team.</returns>
    public double CalculateTeamBiorhythm(
        Dictionary<string, BiorhythmScore> playerBiorhythms,
        Dictionary<string, double> playerWeights)
    {
        if (!playerBiorhythms.Any() || !playerWeights.Any())
        {
            return 0.0; // Neutral score if no data
        }

        double totalWeightedScore = 0.0;
        double totalWeight = 0.0;

        foreach (var playerId in playerBiorhythms.Keys)
        {
            if (playerWeights.TryGetValue(playerId, out var weight))
            {
                totalWeightedScore += playerBiorhythms[playerId].Combined * weight;
                totalWeight += weight;
            }
        }

        return totalWeight > 0 ? totalWeightedScore / totalWeight : 0.0;
    }
}
