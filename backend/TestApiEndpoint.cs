using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DataRetrieval.Services;
using System.Net.Http;

Console.WriteLine("Testing College Football Data API endpoint fix");

// Setup DI
var services = new ServiceCollection();
services.AddHttpClient();
services.AddLogging(builder => builder.AddConsole());

// Add configuration
var configuration = new ConfigurationBuilder()
    .AddJsonFile("/workspace/backend/src/Api/appsettings.json")
    .Build();
services.AddSingleton<IConfiguration>(configuration);

// Add our service
services.AddScoped<ExternalDataService>();

var provider = services.BuildServiceProvider();
var dataService = provider.GetService<ExternalDataService>();

Console.WriteLine("Testing team stats endpoint...");

try
{
    // Test the stats endpoint that we fixed
    var stats = await dataService.GetTeamStatsAsync(2025);
    Console.WriteLine($"Success! Retrieved {stats.Count()} team stats records");
    
    if (stats.Any())
    {
        var firstStat = stats.First();
        Console.WriteLine($"Sample team: {firstStat.TeamName}");
        Console.WriteLine($"Sample stats: Plays={firstStat.Plays}, PPA={firstStat.PPA}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

Console.WriteLine("Test completed.");