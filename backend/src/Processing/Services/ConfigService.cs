using System.Text.Json;
using DataRetrieval.Data;
using DataRetrieval.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Processing.Models;

namespace Processing.Services;

/// <summary>
/// Service for managing prediction configuration settings.
/// Handles storage, retrieval, and updates of model weights and parameters.
/// </summary>
public class ConfigService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ConfigService> _logger;
    private PredictionConfiguration? _cachedConfig;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public ConfigService(ApplicationDbContext context, ILogger<ConfigService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets the currently active prediction configuration.
    /// Uses in-memory caching with 5-minute expiration.
    /// </summary>
    /// <returns>The active configuration, or default if none exists.</returns>
    public async Task<PredictionConfiguration> GetActiveConfigurationAsync()
    {
        // Return cached config if still valid
        if (_cachedConfig != null && DateTime.UtcNow < _cacheExpiry)
        {
            _logger.LogDebug("Returning cached configuration");
            return _cachedConfig;
        }

        try
        {
            var entity = await _context.PredictionConfigurations
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.UpdatedAt)
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                _logger.LogInformation("No active configuration found, creating default");
                entity = await CreateDefaultConfigurationAsync();
            }

            _cachedConfig = MapToModel(entity);
            _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);

            _logger.LogInformation("Loaded active configuration: {Name}", entity.Name);
            return _cachedConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading active configuration, returning default");
            return new PredictionConfiguration();
        }
    }

    /// <summary>
    /// Gets a specific configuration by ID.
    /// </summary>
    /// <param name="configId">Configuration identifier.</param>
    /// <returns>The configuration if found.</returns>
    /// <exception cref="KeyNotFoundException">If configuration does not exist.</exception>
    public async Task<PredictionConfiguration> GetConfigurationByIdAsync(int configId)
    {
        var entity = await _context.PredictionConfigurations
            .FirstOrDefaultAsync(c => c.Id == configId);

        if (entity == null)
        {
            _logger.LogWarning("Configuration {ConfigId} not found", configId);
            throw new KeyNotFoundException($"Configuration with ID {configId} not found");
        }

        return MapToModel(entity);
    }

    /// <summary>
    /// Gets all stored configurations.
    /// </summary>
    /// <returns>List of all configurations.</returns>
    public async Task<List<ConfigurationSummary>> GetAllConfigurationsAsync()
    {
        var entities = await _context.PredictionConfigurations
            .OrderByDescending(c => c.IsActive)
            .ThenByDescending(c => c.UpdatedAt)
            .ToListAsync();

        return entities.Select(e => new ConfigurationSummary
        {
            Id = e.Id,
            Name = e.Name,
            IsActive = e.IsActive,
            StatsWeight = e.StatsWeight,
            BiorhythmWeight = e.BiorhythmWeight,
            HomeFieldAdvantage = e.HomeFieldAdvantage,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            CreatedBy = e.CreatedBy
        }).ToList();
    }

    /// <summary>
    /// Updates the active configuration with new values.
    /// Invalidates cache and marks other configurations as inactive.
    /// </summary>
    /// <param name="config">New configuration values.</param>
    /// <param name="updatedBy">User making the update.</param>
    /// <returns>Updated configuration with ID.</returns>
    public async Task<ConfigurationSummary> UpdateActiveConfigurationAsync(
        PredictionConfiguration config, 
        string? updatedBy = null)
    {
        try
        {
            // Validate weights sum to 1.0
            if (Math.Abs(config.StatsWeight + config.BiorhythmWeight - 1.0) > 0.001)
            {
                throw new ArgumentException(
                    $"Weights must sum to 1.0. Current sum: {config.StatsWeight + config.BiorhythmWeight}");
            }

            // Deactivate all existing configurations
            var activeConfigs = await _context.PredictionConfigurations
                .Where(c => c.IsActive)
                .ToListAsync();

            foreach (var activeConfig in activeConfigs)
            {
                activeConfig.IsActive = false;
                activeConfig.UpdatedAt = DateTime.UtcNow;
            }

            // Create new active configuration
            var entity = new PredictionConfigurationEntity
            {
                Name = $"Config-{DateTime.UtcNow:yyyyMMdd-HHmmss}",
                StatsWeight = config.StatsWeight,
                BiorhythmWeight = config.BiorhythmWeight,
                HomeFieldAdvantage = config.HomeFieldAdvantage,
                PositionImportanceJson = JsonSerializer.Serialize(config.PositionImportance),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = updatedBy ?? "System"
            };

            _context.PredictionConfigurations.Add(entity);
            await _context.SaveChangesAsync();

            // Invalidate cache
            _cachedConfig = null;
            _cacheExpiry = DateTime.MinValue;

            _logger.LogInformation(
                "Updated active configuration to {Name} by {User}. Stats: {Stats}, Bio: {Bio}, Home: {Home}",
                entity.Name, updatedBy, config.StatsWeight, config.BiorhythmWeight, config.HomeFieldAdvantage);

            return new ConfigurationSummary
            {
                Id = entity.Id,
                Name = entity.Name,
                IsActive = entity.IsActive,
                StatsWeight = entity.StatsWeight,
                BiorhythmWeight = entity.BiorhythmWeight,
                HomeFieldAdvantage = entity.HomeFieldAdvantage,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                CreatedBy = entity.CreatedBy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating active configuration");
            throw;
        }
    }

    /// <summary>
    /// Activates a previously saved configuration by ID.
    /// </summary>
    /// <param name="configId">Configuration to activate.</param>
    /// <returns>Activated configuration summary.</returns>
    public async Task<ConfigurationSummary> ActivateConfigurationAsync(int configId)
    {
        var entity = await _context.PredictionConfigurations
            .FirstOrDefaultAsync(c => c.Id == configId);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Configuration {configId} not found");
        }

        // Deactivate all other configurations
        var activeConfigs = await _context.PredictionConfigurations
            .Where(c => c.IsActive && c.Id != configId)
            .ToListAsync();

        foreach (var config in activeConfigs)
        {
            config.IsActive = false;
            config.UpdatedAt = DateTime.UtcNow;
        }

        // Activate target configuration
        entity.IsActive = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Invalidate cache
        _cachedConfig = null;
        _cacheExpiry = DateTime.MinValue;

        _logger.LogInformation("Activated configuration {Name} (ID: {Id})", entity.Name, entity.Id);

        return new ConfigurationSummary
        {
            Id = entity.Id,
            Name = entity.Name,
            IsActive = entity.IsActive,
            StatsWeight = entity.StatsWeight,
            BiorhythmWeight = entity.BiorhythmWeight,
            HomeFieldAdvantage = entity.HomeFieldAdvantage,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy
        };
    }

    /// <summary>
    /// Deletes a configuration by ID.
    /// Cannot delete the active configuration.
    /// </summary>
    /// <param name="configId">Configuration to delete.</param>
    public async Task DeleteConfigurationAsync(int configId)
    {
        var entity = await _context.PredictionConfigurations
            .FirstOrDefaultAsync(c => c.Id == configId);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Configuration {configId} not found");
        }

        if (entity.IsActive)
        {
            throw new InvalidOperationException("Cannot delete the active configuration");
        }

        _context.PredictionConfigurations.Remove(entity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted configuration {Name} (ID: {Id})", entity.Name, entity.Id);
    }

    /// <summary>
    /// Creates the default configuration if none exists.
    /// </summary>
    private async Task<PredictionConfigurationEntity> CreateDefaultConfigurationAsync()
    {
        var defaultConfig = new PredictionConfiguration();
        var entity = new PredictionConfigurationEntity
        {
            Name = "Default",
            StatsWeight = defaultConfig.StatsWeight,
            BiorhythmWeight = defaultConfig.BiorhythmWeight,
            HomeFieldAdvantage = defaultConfig.HomeFieldAdvantage,
            PositionImportanceJson = JsonSerializer.Serialize(defaultConfig.PositionImportance),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        _context.PredictionConfigurations.Add(entity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created default configuration");
        return entity;
    }

    /// <summary>
    /// Maps database entity to domain model.
    /// </summary>
    private PredictionConfiguration MapToModel(PredictionConfigurationEntity entity)
    {
        var positionImportance = string.IsNullOrEmpty(entity.PositionImportanceJson)
            ? new Dictionary<string, double>()
            : JsonSerializer.Deserialize<Dictionary<string, double>>(entity.PositionImportanceJson)
              ?? new Dictionary<string, double>();

        return new PredictionConfiguration
        {
            StatsWeight = entity.StatsWeight,
            BiorhythmWeight = entity.BiorhythmWeight,
            HomeFieldAdvantage = entity.HomeFieldAdvantage,
            PositionImportance = positionImportance
        };
    }
}

/// <summary>
/// Summary information about a stored configuration.
/// </summary>
public class ConfigurationSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public double StatsWeight { get; set; }
    public double BiorhythmWeight { get; set; }
    public double HomeFieldAdvantage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
