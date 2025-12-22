using Api.Controllers;
using DataRetrieval.Data;
using DataRetrieval.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Unit.Tests;

/// <summary>
/// Unit tests for CoreDataController.
/// Tests all core data API endpoints for schedule, teams, stats, and roster access.
/// </summary>
public class CoreDataControllerTests
{
    private readonly Mock<ILogger<CoreDataController>> _mockLogger;

    public CoreDataControllerTests()
    {
        _mockLogger = new Mock<ILogger<CoreDataController>>();
    }

    /// <summary>
    /// Creates an in-memory database context with test data.
    /// </summary>
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        // Seed test data
        SeedTestData(context);

        return context;
    }

    /// <summary>
    /// Seeds the test database with sample data.
    /// </summary>
    private void SeedTestData(ApplicationDbContext context)
    {
        var team1 = new Team
        {
            TeamId = "alabama",
            Name = "Alabama",
            Conference = "SEC"
        };

        var team2 = new Team
        {
            TeamId = "georgia",
            Name = "Georgia",
            Conference = "SEC"
        };

        context.Teams.AddRange(team1, team2);

        var stats1 = new TeamStats
        {
            TeamId = "alabama",
            Season = 2025,
            Ppg = 35.5,
            TotalOffenseRank = 5,
            PassingYardsRank = 10,
            RushingYardsRank = 3
        };

        team1.Stats = stats1;
        context.SaveChanges();

        var player1 = new Player
        {
            PlayerId = "player-001",
            TeamId = "alabama",
            Name = "John Doe",
            Position = "QB",
            DateOfBirth = new DateTime(2003, 5, 15)
        };

        var player2 = new Player
        {
            PlayerId = "player-002",
            TeamId = "alabama",
            Name = "Jane Smith",
            Position = "RB",
            DateOfBirth = new DateTime(2004, 8, 20)
        };

        context.Players.AddRange(player1, player2);

        var game1 = new Game
        {
            GameId = "game-001",
            Date = DateTime.UtcNow.AddDays(7),
            HomeTeamId = "alabama",
            AwayTeamId = "georgia",
            Location = "Bryant-Denny Stadium",
            Status = GameStatus.Scheduled
        };

        var game2 = new Game
        {
            GameId = "game-002",
            Date = DateTime.UtcNow.AddDays(-7),
            HomeTeamId = "georgia",
            AwayTeamId = "alabama",
            Location = "Sanford Stadium",
            Status = GameStatus.Completed,
            HomeScore = 28,
            AwayScore = 24
        };

        context.Games.AddRange(game1, game2);
        context.SaveChanges();
    }

    /// <summary>
    /// Tests that GetUpcomingGames returns only scheduled games.
    /// </summary>
    [Fact]
    public async Task GetUpcomingGames_ReturnsOnlyScheduledGames()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var controller = new CoreDataController(context, _mockLogger.Object);

        // Act
        var result = await controller.GetUpcomingGames();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var games = okResult.Value.Should().BeAssignableTo<List<Game>>().Subject;
        games.Should().HaveCount(1);
        games.All(g => g.Status == GameStatus.Scheduled).Should().BeTrue();
    }

    /// <summary>
    /// Tests that GetGameById returns correct game with teams.
    /// </summary>
    [Fact]
    public async Task GetGameById_ExistingGame_ReturnsGameWithTeams()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var controller = new CoreDataController(context, _mockLogger.Object);

        // Act
        var result = await controller.GetGameById("game-001");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var game = okResult.Value.Should().BeAssignableTo<Game>().Subject;
        game.GameId.Should().Be("game-001");
        game.HomeTeam.Should().NotBeNull();
        game.AwayTeam.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that GetGameById returns NotFound for non-existent game.
    /// </summary>
    [Fact]
    public async Task GetGameById_NonExistentGame_ReturnsNotFound()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var controller = new CoreDataController(context, _mockLogger.Object);

        // Act
        var result = await controller.GetGameById("nonexistent");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    /// <summary>
    /// Tests that GetTeams returns all teams ordered by conference.
    /// </summary>
    [Fact]
    public async Task GetTeams_ReturnsAllTeamsOrderedByConference()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var controller = new CoreDataController(context, _mockLogger.Object);

        // Act
        var result = await controller.GetTeams();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var teams = okResult.Value.Should().BeAssignableTo<List<Team>>().Subject;
        teams.Should().HaveCount(2);
        teams.Should().BeInAscendingOrder(t => t.Conference);
    }

    /// <summary>
    /// Tests that GetTeams filters by conference.
    /// </summary>
    [Fact]
    public async Task GetTeams_WithConferenceFilter_ReturnsFilteredTeams()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var controller = new CoreDataController(context, _mockLogger.Object);

        // Act
        var result = await controller.GetTeams(conference: "SEC");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var teams = okResult.Value.Should().BeAssignableTo<List<Team>>().Subject;
        teams.Should().HaveCount(2);
        teams.All(t => t.Conference == "SEC").Should().BeTrue();
    }

    /// <summary>
    /// Tests that GetTeamStats returns stats for existing team.
    /// </summary>
    [Fact]
    public async Task GetTeamStats_ExistingTeam_ReturnsStats()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var controller = new CoreDataController(context, _mockLogger.Object);

        // Act
        var result = await controller.GetTeamStats(teamId: "alabama");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var stats = okResult.Value.Should().BeAssignableTo<TeamStats>().Subject;
        stats.TeamId.Should().Be("alabama");
    }

    /// <summary>
    /// Tests that GetTeamStats returns NotFound for non-existent team.
    /// </summary>
    [Fact]
    public async Task GetTeamStats_NonExistentTeam_ReturnsNotFound()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var controller = new CoreDataController(context, _mockLogger.Object);

        // Act
        var result = await controller.GetTeamStats(teamId: "nonexistent");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    /// <summary>
    /// Tests that GetTeamRoster returns all players for a team.
    /// </summary>
    [Fact]
    public async Task GetTeamRoster_ExistingTeam_ReturnsRoster()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var controller = new CoreDataController(context, _mockLogger.Object);

        // Act
        var result = await controller.GetTeamRoster(teamId: "alabama");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var roster = okResult.Value.Should().BeAssignableTo<List<Player>>().Subject;
        roster.Should().HaveCount(2);
        roster.All(p => p.TeamId == "alabama").Should().BeTrue();
    }

    /// <summary>
    /// Tests that GetTeamRoster filters by position.
    /// </summary>
    [Fact]
    public async Task GetTeamRoster_WithPositionFilter_ReturnsFilteredPlayers()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var controller = new CoreDataController(context, _mockLogger.Object);

        // Act
        var result = await controller.GetTeamRoster(teamId: "alabama", position: "QB");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var roster = okResult.Value.Should().BeAssignableTo<List<Player>>().Subject;
        roster.Should().HaveCount(1);
        roster.First().Position.Should().Be("QB");
    }

    /// <summary>
    /// Tests that GetTeamRoster returns NotFound for non-existent team.
    /// </summary>
    [Fact]
    public async Task GetTeamRoster_NonExistentTeam_ReturnsNotFound()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var controller = new CoreDataController(context, _mockLogger.Object);

        // Act
        var result = await controller.GetTeamRoster(teamId: "nonexistent");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    /// <summary>
    /// Tests that GetSchedule returns all games.
    /// </summary>
    [Fact]
    public async Task GetSchedule_ReturnsAllGames()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var controller = new CoreDataController(context, _mockLogger.Object);

        // Act
        var result = await controller.GetSchedule();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var games = okResult.Value.Should().BeAssignableTo<List<Game>>().Subject;
        games.Should().HaveCount(2);
    }

    /// <summary>
    /// Tests that GetSchedule filters by team.
    /// </summary>
    [Fact]
    public async Task GetSchedule_WithTeamFilter_ReturnsTeamGames()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var controller = new CoreDataController(context, _mockLogger.Object);

        // Act
        var result = await controller.GetSchedule(teamId: "alabama");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var games = okResult.Value.Should().BeAssignableTo<List<Game>>().Subject;
        games.Should().HaveCount(2);
        games.All(g => g.HomeTeamId == "alabama" || g.AwayTeamId == "alabama").Should().BeTrue();
    }

    /// <summary>
    /// Tests that GetSchedule filters by status.
    /// </summary>
    [Fact]
    public async Task GetSchedule_WithStatusFilter_ReturnsFilteredGames()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var controller = new CoreDataController(context, _mockLogger.Object);

        // Act
        var result = await controller.GetSchedule(status: GameStatus.Completed);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var games = okResult.Value.Should().BeAssignableTo<List<Game>>().Subject;
        games.Should().HaveCount(1);
        games.All(g => g.Status == GameStatus.Completed).Should().BeTrue();
    }

    /// <summary>
    /// Tests that GetGamePredictions returns predictions for a game.
    /// </summary>
    [Fact]
    public async Task GetGamePredictions_ExistingGame_ReturnsPredictions()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        
        // Add a prediction to test game
        var prediction = new Prediction
        {
            GameId = "game-001",
            PredictedWinnerId = "alabama",
            WinProbability = 0.65,
            Margin = 7.5,
            CreatedAt = DateTime.UtcNow
        };
        context.Predictions.Add(prediction);
        await context.SaveChangesAsync();

        var controller = new CoreDataController(context, _mockLogger.Object);

        // Act
        var result = await controller.GetGamePredictions(gameId: "game-001");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var predictions = okResult.Value.Should().BeAssignableTo<List<Prediction>>().Subject;
        predictions.Should().HaveCount(1);
        predictions.First().GameId.Should().Be("game-001");
    }

    /// <summary>
    /// Tests that GetGamePredictions returns NotFound for non-existent game.
    /// </summary>
    [Fact]
    public async Task GetGamePredictions_NonExistentGame_ReturnsNotFound()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var controller = new CoreDataController(context, _mockLogger.Object);

        // Act
        var result = await controller.GetGamePredictions(gameId: "nonexistent");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }
}
