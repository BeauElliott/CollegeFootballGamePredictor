using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataRetrieval.Entities;

/// <summary>
/// Entity for storing prediction configuration settings in the database.
/// Stores weights, parameters, and position importance for the prediction model.
/// </summary>
[Table("prediction_configurations")]
public class PredictionConfigurationEntity
{
    /// <summary>
    /// Unique identifier for the configuration.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Configuration name/version identifier.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "Default";

    /// <summary>
    /// Weight given to traditional statistical analysis (0.0 to 1.0).
    /// </summary>
    [Range(0.0, 1.0)]
    public double StatsWeight { get; set; } = 0.8;

    /// <summary>
    /// Weight given to biorhythm analysis (0.0 to 1.0).
    /// </summary>
    [Range(0.0, 1.0)]
    public double BiorhythmWeight { get; set; } = 0.2;

    /// <summary>
    /// Home field advantage in points.
    /// </summary>
    [Range(0.0, 10.0)]
    public double HomeFieldAdvantage { get; set; } = 3.0;

    /// <summary>
    /// JSON-serialized position importance weights.
    /// </summary>
    [Required]
    [Column(TypeName = "jsonb")]
    public string PositionImportanceJson { get; set; } = "{}";

    /// <summary>
    /// Indicates if this is the currently active configuration.
    /// </summary>
    public bool IsActive { get; set; } = false;

    /// <summary>
    /// When this configuration was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this configuration was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User or system that created this configuration.
    /// </summary>
    [MaxLength(100)]
    public string? CreatedBy { get; set; }
}
