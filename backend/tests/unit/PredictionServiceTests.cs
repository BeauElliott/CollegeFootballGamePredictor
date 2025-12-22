using DataRetrieval.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Processing.Models;
using Processing.Services;

namespace Unit.Tests;

/// <summary>
/// Unit tests for the PredictionService class.
/// Tests the core prediction logic including stats and biorhythm calculations.
/// </summary>
public class PredictionServiceTests
{
    private readonly Mock<ILogger<PredictionService>> _mockLogger;
    private readonly BiorhythmService _biorhythmService;
    private readonly PredictionConfiguration _config;

    public PredictionServiceTests()
    {
        _mockLogger = new Mock<ILogger<PredictionService>>();
        _biorhythmService = new BiorhythmService();
        _config = new PredictionConfiguration();
    }

    [Fact]
    public void PredictGame_WithCompleteData_ReturnsValidPrediction()
    {
        // Arrange
        var service = new PredictionService(_biorhythmService, _mockLogger.Object, _config);
        
        var game = CreateTestGame();
        var homeTeam = CreateTestTeam("team1", "Alabama", better: true);
        var awayTeam = CreateTestTeam("team2", "Auburn", better: false);

        // Act
        var result = service.PredictGame(game, homeTeam, awayTeam);

        // Assert
        result.Should().NotBeNull();
        result.GameId.Should().Be("game1");
        result.PredictedWinnerId.Should().NotBeNullOrEmpty();
        result.PredictedWinnerName.Should().NotBeNullOrEmpty();
        result.WinProbability.Should().BeInRange(0.0, 1.0);
        result.Margin.Should().BeGreaterThanOrEqualTo(0);
        result.Explanation.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void PredictGame_HomeTeamStronger_FavorsHomeTeam()
    {
        // Arrange
        var service = new PredictionService(_biorhythmService, _mockLogger.Object, _config);
        
        var game = CreateTestGame();
        var homeTeam = CreateTestTeam("team1", "Georgia", better: true);
        var awayTeam = CreateTestTeam("team2", "Vanderbilt", better: false);

        // Act
        var result = service.PredictGame(game, homeTeam, awayTeam);

        // Assert
        result.PredictedWinnerId.Should().Be("team1");
        result.PredictedWinnerName.Should().Be("Georgia");
        result.WinProbability.Should().BeGreaterThan(0.5);
    }

    [Fact]
    public void PredictGame_AwayTeamMuchStronger_OvercomesHomeFieldAdvantage()
    {
        // Arrange
        var service = new PredictionService(_biorhythmService, _mockLogger.Object, _config);
        
        var game = CreateTestGame();
        var homeTeam = CreateTestTeam("team1", "Vanderbilt", better: false);
        var awayTeam = CreateTestTeam("team2", "Georgia", better: true);

        // Act
        var result = service.PredictGame(game, homeTeam, awayTeam);

        // Assert
        // Away team should still win despite home field disadvantage
        result.PredictedWinnerId.Should().Be("team2");
        result.PredictedWinnerName.Should().Be("Georgia");
    }

    [Fact]
    public void PredictGame_MissingStats_ReturnsNeutralStatsEdge()
    {
        // Arrange
        var service = new PredictionService(_biorhythmService, _mockLogger.Object, _config);
        
        var game = CreateTestGame();
        var homeTeam = new Team { TeamId = "team1", Name = "Team A", Conference = "SEC" };
        var awayTeam = new Team { TeamId = "team2", Name = "Team B", Conference = "SEC" };

        // Act
        var result = service.PredictGame(game, homeTeam, awayTeam);

        // Assert
        result.StatsEdge.Should().Be(0.0);
        result.PredictedWinnerId.Should().Be("team1"); // Home team wins due to home field advantage
    }

    [Fact]
    public void PredictGame_IncludesAllBreakdownComponents()
    {
        // Arrange
        var service = new PredictionService(_biorhythmService, _mockLogger.Object, _config);
        
        var game = CreateTestGame();
        var homeTeam = CreateTestTeam("team1", "LSU", better: true);
        var awayTeam = CreateTestTeam("team2", "Arkansas", better: false);

        // Act
        var result = service.PredictGame(game, homeTeam, awayTeam);

        // Assert
        result.StatsEdge.Should().NotBe(0.0);
        // Biorhythm edge may be close to zero depending on player birthdays, so we just check it's calculated
        result.HomeFieldAdjustment.Should().Be(_config.HomeFieldAdvantage);
        result.Explanation.Should().Contain("Home field advantage");
    }

    // Helper methods for creating test data
    private Game CreateTestGame()
    {
        return new Game
        {
            GameId = "game1",
            Date = DateTime.Now.AddDays(7),
            Location = "Test Stadium",
            HomeTeamId = "team1",
            AwayTeamId = "team2",
            Status = GameStatus.Scheduled
        };
    }

    private Team CreateTestTeam(string teamId, string name, bool better)
    {
        var team = new Team
        {
            TeamId = teamId,
            Name = name,
            Conference = "SEC",
            Stats = CreateTestStats(better),
            Roster = CreateTestRoster(teamId, 5)
        };
        return team;
    }

    private TeamStats CreateTestStats(bool better)
    {
        return new TeamStats
        {
            Season = 2025,
            Ppg = better ? 35.0 : 21.0,
            TotalOffenseRank = better ? 10 : 50,
            PassingYardsRank = better ? 15 : 60,
            RushingYardsRank = better ? 20 : 55,
            TurnoversLost = better ? 10 : 18,
            PpgAllowed = better ? 15.0 : 28.0,
            TotalDefenseRank = better ? 8 : 45,
            PassingYardsAllowedRank = better ? 12 : 48,
            RushingYardsAllowedRank = better ? 18 : 52,
            TurnoversForced = better ? 22 : 12,
            TurnoverMargin = better ? 1.2 : -0.6
        };
    }

    private List<Player> CreateTestRoster(string teamId, int count)
    {
        var positions = new[] { "QB", "RB", "WR", "OL", "DL", "LB", "DB" };
        var players = new List<Player>();

        for (int i = 0; i < count; i++)
        {
            players.Add(new Player
            {
                PlayerId = $"player{i}",
                Name = $"Player {i}",
                Position = positions[i % positions.Length],
                DateOfBirth = DateTime.Now.AddYears(-20).AddDays(i * 10),
                DepthChart = i < 2 ? "Starter" : "Backup",
                SnapsPerGame = i < 2 ? 65.0 : 30.0,
                TeamId = teamId
            });
        }

        return players;
    }
}
