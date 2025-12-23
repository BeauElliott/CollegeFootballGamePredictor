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
/// Response from College Football Data API for team statistics.
/// </summary>
public class ExternalTeamStatsResponse
{
    public string Team { get; set; } = string.Empty;
    public string Conference { get; set; } = string.Empty;
    public int Season { get; set; }
    public double? PointsPerGame { get; set; }
    public double? PointsAllowed { get; set; }
    public int? TotalOffenseRank { get; set; }
    public int? TotalDefenseRank { get; set; }
    public int? PassingYardsRank { get; set; }
    public int? RushingYardsRank { get; set; }
    public int? PassingYardsAllowedRank { get; set; }
    public int? RushingYardsAllowedRank { get; set; }
    public int? TurnoversLost { get; set; }
    public int? TurnoversGained { get; set; }
    public double? TurnoverMargin { get; set; }
}

/// <summary>
/// Response from College Football Data API for a player.
/// </summary>
public class ExternalPlayerResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Team { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? Height { get; set; }
    public int? Weight { get; set; }
    public int? Jersey { get; set; }
    public int? Year { get; set; }
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
