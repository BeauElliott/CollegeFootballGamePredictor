using DataRetrieval.Data;
using DataRetrieval.Entities;
using DataRetrieval.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Unit.Tests;

/// <summary>
/// Unit tests for the DataRefreshService class.
/// Tests data refresh operations and database updates.
/// </summary>
public class DataRefreshServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ExternalDataService> _mockExternalDataService;
    private readonly Mock<ILogger<DataRefreshService>> _mockLogger;
    private readonly DataRefreshService _service;

    public DataRefreshServiceTests()
    {
        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _mockExternalDataService = new Mock<ExternalDataService>(
            Mock.Of<HttpClient>(),
            Mock.Of<ILogger<ExternalDataService>>(),
            Mock.Of<Microsoft.Extensions.Options.IOptions<DataRetrieval.Configuration.DataSourcesConfiguration>>());
        _mockLogger = new Mock<ILogger<DataRefreshService>>();
        
        _service = new DataRefreshService(_context, _mockExternalDataService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task RefreshTeamsAsync_AddsNewTeams_WhenNotInDatabase()
    {
        // Arrange
        _mockExternalDataService
            .Setup(s => s.GetTeamsAsync())
            .ReturnsAsync(new List<DataRetrieval.Models.ExternalTeamResponse>
            {
                new() { Id = 1, School = "Alabama", Conference = "SEC" },
                new() { Id = 2, School = "Georgia", Conference = "SEC" }
            });

        // Act
        var count = await _service.RefreshTeamsAsync();

        // Assert
        count.Should().Be(2);
        var teams = await _context.Teams.ToListAsync();
        teams.Should().HaveCount(2);
        teams.Should().Contain(t => t.Name == "Alabama");
        teams.Should().Contain(t => t.Name == "Georgia");
    }

    [Fact]
    public async Task RefreshTeamsAsync_UpdatesExistingTeams_WhenAlreadyInDatabase()
    {
        // Arrange
        _context.Teams.Add(new Team
        {
            TeamId = "team-1",
            Name = "Alabama Old",
            Conference = "Old Conference"
        });
        await _context.SaveChangesAsync();

        _mockExternalDataService
            .Setup(s => s.GetTeamsAsync())
            .ReturnsAsync(new List<DataRetrieval.Models.ExternalTeamResponse>
            {
                new() { Id = 1, School = "Alabama", Conference = "SEC" }
            });

        // Act
        var count = await _service.RefreshTeamsAsync();

        // Assert
        count.Should().Be(1);
        var team = await _context.Teams.FindAsync("team-1");
        team.Should().NotBeNull();
        team!.Name.Should().Be("Alabama");
        team.Conference.Should().Be("SEC");
    }

    [Fact]
    public async Task RefreshScheduleAsync_AddsNewGames_WhenNotInDatabase()
    {
        // Arrange
        var season = 2025;
        
        // Add required teams first
        _context.Teams.AddRange(
            new Team { TeamId = "team-Alabama", Name = "Alabama", Conference = "SEC" },
            new Team { TeamId = "team-Georgia", Name = "Georgia", Conference = "SEC" }
        );
        await _context.SaveChangesAsync();

        _mockExternalDataService
            .Setup(s => s.GetGamesAsync(season, null))
            .ReturnsAsync(new List<DataRetrieval.Models.ExternalGameResponse>
            {
                new()
                {
                    Id = 1,
                    HomeTeam = "Alabama",
                    AwayTeam = "Georgia",
                    Venue = "Bryant-Denny Stadium",
                    StartDate = new DateTime(2025, 9, 15),
                    Completed = false
                }
            });

        // Act
        var count = await _service.RefreshScheduleAsync(season);

        // Assert
        count.Should().Be(1);
        var games = await _context.Games.ToListAsync();
        games.Should().HaveCount(1);
        games[0].Location.Should().Be("Bryant-Denny Stadium");
        games[0].Status.Should().Be(GameStatus.Scheduled);
    }

    [Fact]
    public async Task RefreshTeamStatsAsync_AddsNewStats_WhenNotInDatabase()
    {
        // Arrange
        var season = 2025;
        
        // Add required team first
        _context.Teams.Add(new Team
        {
            TeamId = "team-Alabama",
            Name = "Alabama",
            Conference = "SEC"
        });
        await _context.SaveChangesAsync();

        _mockExternalDataService
            .Setup(s => s.GetTeamStatsAsync(season))
            .ReturnsAsync(new List<DataRetrieval.Models.ExternalTeamStatsResponse>
            {
                new()
                {
                    Team = "Alabama",
                    Season = season,
                    Offense = new DataRetrieval.Models.OffenseStats
                    {
                        Ppa = 0.35,
                        Plays = 800
                    },
                    Defense = new DataRetrieval.Models.DefenseStats
                    {
                        Ppa = 0.15,
                        Plays = 650
                    }
                }
            });

        // Act
        var count = await _service.RefreshTeamStatsAsync(season);

        // Assert
        count.Should().Be(1);
        var stats = await _context.TeamStats.ToListAsync();
        stats.Should().HaveCount(1);
        stats[0].Ppg.Should().Be(35.5);
        stats[0].TotalOffenseRank.Should().Be(5);
    }

    [Fact]
    public async Task RefreshAllAsync_RefreshesAllData_Successfully()
    {
        // Arrange
        var season = 2025;
        
        _mockExternalDataService
            .Setup(s => s.GetTeamsAsync())
            .ReturnsAsync(new List<DataRetrieval.Models.ExternalTeamResponse>
            {
                new() { Id = 1, School = "Alabama", Conference = "SEC" }
            });

        _mockExternalDataService
            .Setup(s => s.GetGamesAsync(season, null))
            .ReturnsAsync(new List<DataRetrieval.Models.ExternalGameResponse>());

        _mockExternalDataService
            .Setup(s => s.GetTeamStatsAsync(season))
            .ReturnsAsync(new List<DataRetrieval.Models.ExternalTeamStatsResponse>());

        // Act
        var result = await _service.RefreshAllAsync(season);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.TeamsUpdated.Should().Be(1);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
