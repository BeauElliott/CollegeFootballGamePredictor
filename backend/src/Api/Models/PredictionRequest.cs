using System.ComponentModel.DataAnnotations;

namespace Api.Models;

/// <summary>
/// Request to predict the outcome of a game.
/// </summary>
public class PredictionRequest
{
    /// <summary>
    /// Unique identifier of the game to predict.
    /// </summary>
    [Required]
    public string GameId { get; set; } = string.Empty;
}
