using Api.Middleware;
using DataRetrieval.Configuration;
using DataRetrieval.Data;
using DataRetrieval.Services;
using Microsoft.EntityFrameworkCore;
using Processing.Models;
using Processing.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Controllers
builder.Services.AddControllers();

// Configure DataSources settings
builder.Services.Configure<DataSourcesConfiguration>(
    builder.Configuration.GetSection(DataSourcesConfiguration.SectionName));

// Register HTTP client for external data service
builder.Services.AddHttpClient<ExternalDataService>();

// Register data services
builder.Services.AddScoped<ExternalDataService>();
builder.Services.AddScoped<DataRefreshService>();

// Register prediction services
builder.Services.AddScoped<BiorhythmService>();
builder.Services.AddScoped<PredictionService>(provider =>
{
    var biorhythmService = provider.GetRequiredService<BiorhythmService>();
    var logger = provider.GetRequiredService<ILogger<PredictionService>>();
    var config = new PredictionConfiguration(); // TODO: Load from configuration
    return new PredictionService(biorhythmService, logger, config);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add custom middleware
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

// Make the implicit Program class accessible for integration tests
public partial class Program { }

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
