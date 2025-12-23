using FluentAssertions;
using Processing.Services;

namespace Unit.Tests;

/// <summary>
/// Unit tests for the BiorhythmService class.
/// Tests the biorhythm calculation logic.
/// </summary>
public class BiorhythmServiceTests
{
    private readonly BiorhythmService _service;

    public BiorhythmServiceTests()
    {
        _service = new BiorhythmService();
    }

    [Fact]
    public void CalculateBiorhythm_ReturnsScoreInValidRange()
    {
        // Arrange
        var dateOfBirth = new DateTime(2000, 1, 1);
        var targetDate = new DateTime(2025, 12, 22);

        // Act
        var result = _service.CalculateBiorhythm(dateOfBirth, targetDate);

        // Assert
        result.Physical.Should().BeInRange(-1.0, 1.0);
        result.Emotional.Should().BeInRange(-1.0, 1.0);
        result.Intellectual.Should().BeInRange(-1.0, 1.0);
        result.Combined.Should().BeInRange(-1.0, 1.0);
    }

    [Fact]
    public void CalculateBiorhythm_SameDateOfBirth_ProducesDifferentScoresOnDifferentDates()
    {
        // Arrange
        var dateOfBirth = new DateTime(2000, 6, 15);
        var date1 = new DateTime(2025, 12, 1);
        var date2 = new DateTime(2025, 12, 15);

        // Act
        var result1 = _service.CalculateBiorhythm(dateOfBirth, date1);
        var result2 = _service.CalculateBiorhythm(dateOfBirth, date2);

        // Assert
        result1.Combined.Should().NotBe(result2.Combined);
    }

    [Fact]
    public void CalculateTeamBiorhythm_WithEmptyData_ReturnsNeutral()
    {
        // Arrange
        var emptyBiorhythms = new Dictionary<string, Processing.Models.BiorhythmScore>();
        var emptyWeights = new Dictionary<string, double>();

        // Act
        var result = _service.CalculateTeamBiorhythm(emptyBiorhythms, emptyWeights);

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void CalculateTeamBiorhythm_WithWeightedPlayers_ReturnsWeightedAverage()
    {
        // Arrange
        var biorhythms = new Dictionary<string, Processing.Models.BiorhythmScore>
        {
            { "player1", new Processing.Models.BiorhythmScore { Combined = 0.8 } },
            { "player2", new Processing.Models.BiorhythmScore { Combined = -0.4 } }
        };
        
        var weights = new Dictionary<string, double>
        {
            { "player1", 1.0 },  // High importance
            { "player2", 0.5 }   // Lower importance
        };

        // Act
        var result = _service.CalculateTeamBiorhythm(biorhythms, weights);

        // Assert
        // Expected: (0.8 * 1.0 + (-0.4) * 0.5) / (1.0 + 0.5) = 0.6 / 1.5 = 0.4
        result.Should().BeApproximately(0.4, 0.01);
    }
}
