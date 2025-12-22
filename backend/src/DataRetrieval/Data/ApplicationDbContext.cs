using DataRetrieval.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataRetrieval.Data;

/// <summary>
/// Entity Framework Core database context for the College Football Game Predictor application.
/// Manages database connections and entity mappings for PostgreSQL.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// College football games.
    /// </summary>
    public DbSet<Game> Games { get; set; } = null!;

    /// <summary>
    /// College football teams.
    /// </summary>
    public DbSet<Team> Teams { get; set; } = null!;

    /// <summary>
    /// Team statistics.
    /// </summary>
    public DbSet<TeamStats> TeamStats { get; set; } = null!;

    /// <summary>
    /// Players on team rosters.
    /// </summary>
    public DbSet<Player> Players { get; set; } = null!;

    /// <summary>
    /// Game outcome predictions.
    /// </summary>
    public DbSet<Prediction> Predictions { get; set; } = null!;

    /// <summary>
    /// Prediction breakdown details.
    /// </summary>
    public DbSet<PredictionBreakdown> PredictionBreakdowns { get; set; } = null!;

    /// <summary>
    /// Prediction configuration settings.
    /// </summary>
    public DbSet<PredictionConfigurationEntity> PredictionConfigurations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Game entity relationships
        modelBuilder.Entity<Game>()
            .HasOne(g => g.HomeTeam)
            .WithMany(t => t.HomeGames)
            .HasForeignKey(g => g.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Game>()
            .HasOne(g => g.AwayTeam)
            .WithMany(t => t.AwayGames)
            .HasForeignKey(g => g.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Team-TeamStats one-to-one relationship
        modelBuilder.Entity<Team>()
            .HasOne(t => t.Stats)
            .WithOne(s => s.Team)
            .HasForeignKey<TeamStats>(s => s.TeamId);

        // Configure Prediction-PredictionBreakdown one-to-one relationship
        modelBuilder.Entity<Prediction>()
            .HasOne(p => p.Breakdown)
            .WithOne(b => b.Prediction)
            .HasForeignKey<PredictionBreakdown>(b => b.PredictionId);

        // Add indexes for common queries
        modelBuilder.Entity<Game>()
            .HasIndex(g => g.Date);

        modelBuilder.Entity<Game>()
            .HasIndex(g => g.Status);

        modelBuilder.Entity<Player>()
            .HasIndex(p => p.TeamId);

        modelBuilder.Entity<Prediction>()
            .HasIndex(p => p.GameId);
    }
}
