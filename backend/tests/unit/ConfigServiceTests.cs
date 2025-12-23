using DataRetrieval.Data;
using DataRetrieval.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Processing.Models;
using Processing.Services;
using Xunit;

namespace Unit.Tests;

/// <summary>
/// Unit tests for ConfigService.
/// Tests configuration storage, retrieval, activation, and validation logic.
/// </summary>
public class ConfigServiceTests
{
    private readonly Mock<ILogger<ConfigService>> _mockLogger;

    public ConfigServiceTests()
    {
        _mockLogger = new Mock<ILogger<ConfigService>>();
    }

    /// <summary>
    /// Creates an in-memory database context for testing.
    /// </summary>
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Tests that GetActiveConfiguration returns default config when none exists.
    /// </summary>
    [Fact]
    public async Task GetActiveConfiguration_NoConfigExists_ReturnsDefault()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var service = new ConfigService(context, _mockLogger.Object);

        // Act
        var config = await service.GetActiveConfigurationAsync();

        // Assert
        config.Should().NotBeNull();
        config.StatsWeight.Should().Be(0.8);
        config.BiorhythmWeight.Should().Be(0.2);
        config.HomeFieldAdvantage.Should().Be(3.0);
        config.PositionImportance.Should().ContainKey("QB");

        // Verify default was created in database
        var dbConfig = await context.PredictionConfigurations.FirstOrDefaultAsync();
        dbConfig.Should().NotBeNull();
        dbConfig!.IsActive.Should().BeTrue();
        dbConfig.Name.Should().Be("Default");
    }

    /// <summary>
    /// Tests that GetActiveConfiguration returns cached config on second call.
    /// </summary>
    [Fact]
    public async Task GetActiveConfiguration_CalledTwice_UsesCaching()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var service = new ConfigService(context, _mockLogger.Object);

        // Act
        var config1 = await service.GetActiveConfigurationAsync();
        var config2 = await service.GetActiveConfigurationAsync();

        // Assert
        config1.Should().BeSameAs(config2); // Same instance = cached
    }

    /// <summary>
    /// Tests that UpdateActiveConfiguration creates new version and deactivates old.
    /// </summary>
    [Fact]
    public async Task UpdateActiveConfiguration_CreatesNewVersionAndDeactivatesOld()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var service = new ConfigService(context, _mockLogger.Object);

        // Create initial config
        await service.GetActiveConfigurationAsync();

        var newConfig = new PredictionConfiguration
        {
            StatsWeight = 0.7,
            BiorhythmWeight = 0.3,
            HomeFieldAdvantage = 4.0
        };

        // Act
        var result = await service.UpdateActiveConfigurationAsync(newConfig, "TestUser");

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeTrue();
        result.StatsWeight.Should().Be(0.7);
        result.BiorhythmWeight.Should().Be(0.3);
        result.HomeFieldAdvantage.Should().Be(4.0);
        result.CreatedBy.Should().Be("TestUser");

        // Verify old config was deactivated
        var allConfigs = await context.PredictionConfigurations.ToListAsync();
        allConfigs.Should().HaveCount(2);
        allConfigs.Count(c => c.IsActive).Should().Be(1);
        allConfigs.First(c => c.Name == "Default").IsActive.Should().BeFalse();
    }

    /// <summary>
    /// Tests that UpdateActiveConfiguration validates weights sum to 1.0.
    /// </summary>
    [Fact]
    public async Task UpdateActiveConfiguration_WeightsDontSumToOne_ThrowsException()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var service = new ConfigService(context, _mockLogger.Object);

        var invalidConfig = new PredictionConfiguration
        {
            StatsWeight = 0.6,
            BiorhythmWeight = 0.3, // Sum = 0.9, should be 1.0
            HomeFieldAdvantage = 3.0
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateActiveConfigurationAsync(invalidConfig));
    }

    /// <summary>
    /// Tests that ActivateConfiguration switches active config.
    /// </summary>
    [Fact]
    public async Task ActivateConfiguration_SwitchesActiveConfig()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var service = new ConfigService(context, _mockLogger.Object);

        // Create two configs
        await service.GetActiveConfigurationAsync();
        var config2 = await service.UpdateActiveConfigurationAsync(
            new PredictionConfiguration { StatsWeight = 0.7, BiorhythmWeight = 0.3 });

        var allConfigs = await context.PredictionConfigurations.ToListAsync();
        var firstConfigId = allConfigs.First(c => c.Name == "Default").Id;

        // Act
        var result = await service.ActivateConfigurationAsync(firstConfigId);

        // Assert
        result.IsActive.Should().BeTrue();
        result.Id.Should().Be(firstConfigId);

        // Verify config2 was deactivated
        var config2Entity = await context.PredictionConfigurations
            .FirstAsync(c => c.Id == config2.Id);
        config2Entity.IsActive.Should().BeFalse();
    }

    /// <summary>
    /// Tests that ActivateConfiguration throws when config not found.
    /// </summary>
    [Fact]
    public async Task ActivateConfiguration_ConfigNotFound_ThrowsException()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var service = new ConfigService(context, _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.ActivateConfigurationAsync(999));
    }

    /// <summary>
    /// Tests that GetAllConfigurations returns all configs sorted by active then date.
    /// </summary>
    [Fact]
    public async Task GetAllConfigurations_ReturnsAllSortedByActiveThenDate()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var service = new ConfigService(context, _mockLogger.Object);

        // Create multiple configs
        await service.GetActiveConfigurationAsync();
        await service.UpdateActiveConfigurationAsync(
            new PredictionConfiguration { StatsWeight = 0.7, BiorhythmWeight = 0.3 });
        await service.UpdateActiveConfigurationAsync(
            new PredictionConfiguration { StatsWeight = 0.6, BiorhythmWeight = 0.4 });

        // Act
        var configs = await service.GetAllConfigurationsAsync();

        // Assert
        configs.Should().HaveCount(3);
        configs.First().IsActive.Should().BeTrue(); // Active config should be first
        configs.Should().BeInDescendingOrder(c => c.UpdatedAt); // Within active/inactive groups
    }

    /// <summary>
    /// Tests that DeleteConfiguration removes non-active config.
    /// </summary>
    [Fact]
    public async Task DeleteConfiguration_NonActiveConfig_RemovesSuccessfully()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var service = new ConfigService(context, _mockLogger.Object);

        await service.GetActiveConfigurationAsync();
        await service.UpdateActiveConfigurationAsync(
            new PredictionConfiguration { StatsWeight = 0.7, BiorhythmWeight = 0.3 });

        var allConfigs = await context.PredictionConfigurations.ToListAsync();
        var inactiveConfigId = allConfigs.First(c => !c.IsActive).Id;

        // Act
        await service.DeleteConfigurationAsync(inactiveConfigId);

        // Assert
        var remainingConfigs = await context.PredictionConfigurations.ToListAsync();
        remainingConfigs.Should().HaveCount(1);
        remainingConfigs.Should().NotContain(c => c.Id == inactiveConfigId);
    }

    /// <summary>
    /// Tests that DeleteConfiguration throws when trying to delete active config.
    /// </summary>
    [Fact]
    public async Task DeleteConfiguration_ActiveConfig_ThrowsException()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var service = new ConfigService(context, _mockLogger.Object);

        await service.GetActiveConfigurationAsync();
        var activeConfig = await context.PredictionConfigurations.FirstAsync(c => c.IsActive);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DeleteConfigurationAsync(activeConfig.Id));
    }

    /// <summary>
    /// Tests that GetConfigurationById returns correct config.
    /// </summary>
    [Fact]
    public async Task GetConfigurationById_ExistingConfig_ReturnsConfig()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var service = new ConfigService(context, _mockLogger.Object);

        var summary = await service.UpdateActiveConfigurationAsync(
            new PredictionConfiguration { StatsWeight = 0.75, BiorhythmWeight = 0.25 });

        // Act
        var config = await service.GetConfigurationByIdAsync(summary.Id);

        // Assert
        config.Should().NotBeNull();
        config.StatsWeight.Should().Be(0.75);
        config.BiorhythmWeight.Should().Be(0.25);
    }

    /// <summary>
    /// Tests that GetConfigurationById throws when config not found.
    /// </summary>
    [Fact]
    public async Task GetConfigurationById_NonExistentConfig_ThrowsException()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var service = new ConfigService(context, _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.GetConfigurationByIdAsync(999));
    }
}
