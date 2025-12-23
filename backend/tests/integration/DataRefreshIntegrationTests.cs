using System.Net;
using System.Net.Http.Json;
using DataRetrieval.Data;
using DataRetrieval.Models;
using DataRetrieval.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

/// <summary>
/// Integration tests for data refresh operations.
/// Tests the end-to-end flow of refreshing data from external sources.
/// </summary>
public class DataRefreshIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DataRefreshIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove all DbContext-related registrations
                var descriptors = services.Where(
                    d => d.ServiceType == typeof(ApplicationDbContext) ||
                         d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                         d.ServiceType == typeof(DbContextOptions)).ToList();
                
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // Add DbContext using in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb");
                });
            });

            builder.UseEnvironment("Testing");
        });
    }

    /// <summary>
    /// Tests that health endpoint returns OK status.
    /// Validates that the API is up and running.
    /// </summary>
    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("status"));
    }

    /// <summary>
    /// Tests that refresh teams endpoint returns success.
    /// Note: This test will fail if external API is not configured or unavailable.
    /// Consider mocking external service for true integration tests.
    /// </summary>
    [Fact(Skip = "Requires external API configuration")]
    public async Task RefreshTeams_WithValidConfig_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/admin/refresh/teams", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DataRefreshResult>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.True(result.TeamsUpdated > 0);
    }

    /// <summary>
    /// Tests that refresh schedule endpoint returns success.
    /// Note: This test will fail if external API is not configured or unavailable.
    /// </summary>
    [Fact(Skip = "Requires external API configuration")]
    public async Task RefreshSchedule_WithValidConfig_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/admin/refresh/schedule?season=2025&week=1", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DataRefreshResult>();
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    /// <summary>
    /// Tests that refresh stats endpoint returns success.
    /// Note: This test will fail if external API is not configured or unavailable.
    /// </summary>
    [Fact(Skip = "Requires external API configuration")]
    public async Task RefreshStats_WithValidConfig_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/admin/refresh/stats?season=2025", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DataRefreshResult>();
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    /// <summary>
    /// Tests that refresh all endpoint returns success.
    /// Note: This test will fail if external API is not configured or unavailable.
    /// </summary>
    [Fact(Skip = "Requires external API configuration")]
    public async Task RefreshAll_WithValidConfig_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/admin/refresh/all?season=2025", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DataRefreshResult>();
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    /// <summary>
    /// Tests that admin endpoints require proper authorization.
    /// This is a placeholder - actual auth implementation pending.
    /// </summary>
    [Fact(Skip = "Authorization not yet implemented")]
    public async Task AdminEndpoints_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        // Remove any default auth headers

        // Act
        var response = await client.PostAsync("/api/admin/refresh/teams", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
