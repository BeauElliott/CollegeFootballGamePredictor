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

    // DbSet properties for entities will be added as they are created
    // Example: public DbSet<Game> Games { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Entity configurations will be added here as models are created
        // Example: modelBuilder.ApplyConfiguration(new GameConfiguration());
    }
}
