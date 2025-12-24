using Api.Models;
using DataRetrieval.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Processing.Services;

namespace Api.Controllers;

/// <summary>
/// Controller for game prediction operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PredictionController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PredictionService _predictionService;
    private readonly ILogger<PredictionController> _logger;

    public PredictionController(
        ApplicationDbContext context,
        PredictionService predictionService,
        ILogger<PredictionController> logger)
    {
        _context = context;
        _predictionService = predictionService;
        _logger = logger;
    }

    /// <summary>
    /// Generates a prediction for a specified game.
    /// </summary>
    /// <param name="request">Prediction request containing the game ID.</param>
    /// <returns>Prediction result with winner, probability, and breakdown.</returns>
    /// <response code="200">Returns the prediction result.</response>
    /// <response code="400">If the game ID is invalid or missing.</response>
    /// <response code="404">If the game is not found.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PredictionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PredictionResponse>> PredictGame([FromBody] PredictionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.GameId))
        {
            _logger.LogWarning("Prediction request received with empty game ID");
            return BadRequest(new { error = "Game ID is required" });
        }

        _logger.LogInformation("Prediction requested for game {GameId}", request.GameId);

        // Fetch game with related teams and stats (use AsSplitQuery to avoid cartesian explosion)
        var game = await _context.Games
            .Include(g => g.HomeTeam)
                .ThenInclude(t => t!.Stats)
            .Include(g => g.AwayTeam)
                .ThenInclude(t => t!.Stats)
            .AsSplitQuery()
            .FirstOrDefaultAsync(g => g.GameId == request.GameId);

        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found", request.GameId);
            return NotFound(new { error = $"Game with ID '{request.GameId}' not found" });
        }

        if (game.HomeTeam == null || game.AwayTeam == null)
        {
            _logger.LogError("Game {GameId} has missing team data", request.GameId);
            return BadRequest(new { error = "Game has incomplete team data" });
        }

        // Load rosters separately to avoid slow queries
        var homeRoster = await _context.Players
            .Where(p => p.TeamId == game.HomeTeamId)
            .ToListAsync();
        
        var awayRoster = await _context.Players
            .Where(p => p.TeamId == game.AwayTeamId)
            .ToListAsync();

        // Assign rosters to teams
        game.HomeTeam.Roster = homeRoster;
        game.AwayTeam.Roster = awayRoster;

        // Generate prediction
        var predictionResult = _predictionService.PredictGame(game, game.HomeTeam, game.AwayTeam);

        // Save prediction to database
        var prediction = new DataRetrieval.Entities.Prediction
        {
            GameId = game.GameId,
            PredictedWinnerId = predictionResult.PredictedWinnerId,
            WinProbability = predictionResult.WinProbability,
            Margin = predictionResult.Margin,
            CreatedAt = DateTime.UtcNow,
            Breakdown = new DataRetrieval.Entities.PredictionBreakdown
            {
                StatsEdge = predictionResult.StatsEdge,
                BiorhythmEdge = predictionResult.BiorhythmEdge,
                HomeFieldAdjustment = predictionResult.HomeFieldAdjustment,
                Explanation = predictionResult.Explanation
            }
        };

        // Check if prediction already exists, update if so
        var existingPrediction = await _context.Predictions
            .Include(p => p.Breakdown)
            .FirstOrDefaultAsync(p => p.GameId == request.GameId);

        if (existingPrediction != null)
        {
            existingPrediction.PredictedWinnerId = prediction.PredictedWinnerId;
            existingPrediction.WinProbability = prediction.WinProbability;
            existingPrediction.Margin = prediction.Margin;
            existingPrediction.UpdatedAt = DateTime.UtcNow;
            
            if (existingPrediction.Breakdown != null)
            {
                existingPrediction.Breakdown.StatsEdge = predictionResult.StatsEdge;
                existingPrediction.Breakdown.BiorhythmEdge = predictionResult.BiorhythmEdge;
                existingPrediction.Breakdown.HomeFieldAdjustment = predictionResult.HomeFieldAdjustment;
                existingPrediction.Breakdown.Explanation = predictionResult.Explanation;
            }
        }
        else
        {
            _context.Predictions.Add(prediction);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Prediction saved for game {GameId}: {Winner} with {Probability:P0} probability",
            request.GameId, predictionResult.PredictedWinnerName, predictionResult.WinProbability);

        // Return response
        var response = new PredictionResponse
        {
            GameId = predictionResult.GameId,
            PredictedWinner = predictionResult.PredictedWinnerName,
            PredictedWinnerId = predictionResult.PredictedWinnerId,
            WinProbability = predictionResult.WinProbability,
            Margin = predictionResult.Margin,
            Timestamp = DateTime.UtcNow,
            Breakdown = new PredictionBreakdownResponse
            {
                StatsEdge = predictionResult.StatsEdge,
                BiorhythmEdge = predictionResult.BiorhythmEdge,
                HomeFieldAdvantage = predictionResult.HomeFieldAdjustment,
                Explanation = predictionResult.Explanation
            }
        };

        return Ok(response);
    }
}
