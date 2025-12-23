namespace DataRetrieval.Configuration;

/// <summary>
/// Configuration settings for external data sources.
/// </summary>
public class DataSourcesConfiguration
{
    public const string SectionName = "DataSources";

    public DataSourceSettings CollegeFootballData { get; set; } = new();
    public DataSourceSettings ESPN { get; set; } = new();
    public DataSourceSettings SportsReference { get; set; } = new();
}

/// <summary>
/// Settings for an individual data source.
/// </summary>
public class DataSourceSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int RateLimitPerMinute { get; set; } = 60;
    public bool Enabled { get; set; } = true;
    public string Note { get; set; } = string.Empty;
}
