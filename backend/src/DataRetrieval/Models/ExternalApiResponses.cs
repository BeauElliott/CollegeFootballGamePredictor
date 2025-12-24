namespace DataRetrieval.Models;

/// <summary>
/// Response from College Football Data API for a game.
/// </summary>
public class ExternalGameResponse
{
    public int Id { get; set; }
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public int? HomePoints { get; set; }
    public int? AwayPoints { get; set; }
    public bool Completed { get; set; }
}

/// <summary>
/// Response from College Football Data API for team statistics (advanced stats endpoint).
/// </summary>
public class ExternalTeamStatsResponse
{
    public string Team { get; set; } = string.Empty;
    public string Conference { get; set; } = string.Empty;
    public int Season { get; set; }
    public OffenseStats? Offense { get; set; }
    public DefenseStats? Defense { get; set; }
}

public class OffenseStats
{
    public int? Plays { get; set; }
    public double? Ppa { get; set; }
    public double? SuccessRate { get; set; }
    public double? Explosiveness { get; set; }
    public PlayStats? RushingPlays { get; set; }
    public PlayStats? PassingPlays { get; set; }
}

public class DefenseStats
{
    public int? Plays { get; set; }
    public double? Ppa { get; set; }
    public double? SuccessRate { get; set; }
    public double? Explosiveness { get; set; }
    public PlayStats? RushingPlays { get; set; }
    public PlayStats? PassingPlays { get; set; }
}

public class PlayStats
{
    public double? Rate { get; set; }
    public double? Ppa { get; set; }
    public double? TotalPPA { get; set; }
    public double? SuccessRate { get; set; }
}

/// <summary>
/// Response from College Football Data API for a player.
/// </summary>
public class ExternalPlayerResponse
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Team { get; set; } = string.Empty;
    public string? Position { get; set; }
    public int? Height { get; set; }
    public int? Weight { get; set; }
    public int? Jersey { get; set; }
    public int? Year { get; set; }
    
    // Computed property to get full name
    public string FullName => $"{FirstName} {LastName}".Trim();
}

/// <summary>
/// Response from College Football Data API for player usage statistics.
/// </summary>
public class ExternalPlayerUsageResponse
{
    public int Season { get; set; }
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Team { get; set; } = string.Empty;
    public string Conference { get; set; } = string.Empty;
    public UsageStats? Usage { get; set; }
}

public class UsageStats
{
    public double? Overall { get; set; }
    public double? Pass { get; set; }
    public double? Rush { get; set; }
    public double? FirstDown { get; set; }
    public double? SecondDown { get; set; }
    public double? ThirdDown { get; set; }
    public double? StandardDowns { get; set; }
    public double? PassingDowns { get; set; }
}

/// <summary>
/// Response from College Football Data API for a team.
/// </summary>
public class ExternalTeamResponse
{
    public int Id { get; set; }
    public string School { get; set; } = string.Empty;
    public string Mascot { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string Conference { get; set; } = string.Empty;
    public string? Division { get; set; }
    public string? Color { get; set; }
    public string? AltColor { get; set; }
}
