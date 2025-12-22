using DataRetrieval.Entities;
using Microsoft.Extensions.Logging;
using Processing.Models;

namespace Processing.Services;

/// <summary>
/// Service for generating game outcome predictions based on team statistics and player biorhythms.
/// Combines traditional statistical analysis with experimental biorhythm-based player performance prediction.
/// </summary>
public class PredictionService
{
    private readonly BiorhythmService _biorhythmService;
    private readonly ILogger<PredictionService> _logger;
    private readonly PredictionConfiguration _config;

    public PredictionService(
        BiorhythmService biorhythmService,
        ILogger<PredictionService> logger,
        PredictionConfiguration? config = null)
    {
        _biorhythmService = biorhythmService;
        _logger = logger;
        _config = config ?? new PredictionConfiguration();
    }

    /// <summary>
    /// Generates a prediction for a game between two teams.
    /// </summary>
    /// <param name="game">The game to predict.</param>
    /// <param name="homeTeam">Home team with stats and roster.</param>
    /// <param name="awayTeam">Away team with stats and roster.</param>
    /// <returns>Prediction result with winner, probability, and breakdown.</returns>
    public PredictionResult PredictGame(Game game, Team homeTeam, Team awayTeam)
    {
        _logger.LogInformation(
            "Generating prediction for game {GameId}: {HomeTeam} vs {AwayTeam}",
            game.GameId, homeTeam.Name, awayTeam.Name);

        // Calculate statistical edge
        var statsEdge = CalculateStatsEdge(homeTeam.Stats, awayTeam.Stats);
        _logger.LogDebug("Stats edge: {StatsEdge}", statsEdge);

        // Calculate biorhythm edge
        var biorhythmEdge = CalculateBiorhythmEdge(
            homeTeam.Roster.ToList(),
            awayTeam.Roster.ToList(),
            game.Date);
        _logger.LogDebug("Biorhythm edge: {BiorhythmEdge}", biorhythmEdge);

        // Apply weights to calculate combined score
        var weightedStatsEdge = statsEdge * _config.StatsWeight;
        var weightedBiorhythmEdge = biorhythmEdge * _config.BiorhythmWeight;
        var combinedEdge = weightedStatsEdge + weightedBiorhythmEdge;

        // Apply home field advantage
        var homeFieldAdjustment = _config.HomeFieldAdvantage;
        var totalEdge = combinedEdge + homeFieldAdjustment;

        // Determine winner and probability
        var predictedWinnerId = totalEdge >= 0 ? homeTeam.TeamId : awayTeam.TeamId;
        var predictedWinnerName = totalEdge >= 0 ? homeTeam.Name : awayTeam.Name;
        var margin = Math.Abs(totalEdge);
        
        // Convert margin to probability using logistic function
        // Margin of ~14 points gives ~75% probability
        var winProbability = 1.0 / (1.0 + Math.Exp(-totalEdge / 7.0));

        // Generate explanation
        var explanation = GenerateExplanation(
            homeTeam.Name,
            awayTeam.Name,
            statsEdge,
            biorhythmEdge,
            homeFieldAdjustment,
            predictedWinnerName,
            margin);

        _logger.LogInformation(
            "Prediction complete: {Winner} wins with {Probability:P0} probability",
            predictedWinnerName, winProbability);

        return new PredictionResult
        {
            GameId = game.GameId,
            PredictedWinnerId = predictedWinnerId,
            PredictedWinnerName = predictedWinnerName,
            WinProbability = winProbability,
            Margin = margin,
            StatsEdge = statsEdge,
            BiorhythmEdge = biorhythmEdge,
            HomeFieldAdjustment = homeFieldAdjustment,
            Explanation = explanation
        };
    }

    /// <summary>
    /// Calculates statistical advantage between two teams.
    /// Positive values favor home team, negative values favor away team.
    /// </summary>
    private double CalculateStatsEdge(TeamStats? homeStats, TeamStats? awayStats)
    {
        if (homeStats == null || awayStats == null)
        {
            _logger.LogWarning("Missing team stats, returning neutral stats edge");
            return 0.0;
        }

        // Offensive advantage (lower rank is better)
        var offensiveEdge = (awayStats.TotalOffenseRank - homeStats.TotalOffenseRank) * 0.1;
        
        // Defensive advantage (lower rank is better)
        var defensiveEdge = (awayStats.TotalDefenseRank - homeStats.TotalDefenseRank) * 0.1;
        
        // Scoring advantage
        var scoringEdge = (homeStats.Ppg - awayStats.Ppg) * 0.5;
        var scoringDefenseEdge = (awayStats.PpgAllowed - homeStats.PpgAllowed) * 0.5;
        
        // Turnover margin advantage
        var turnoverEdge = (homeStats.TurnoverMargin - awayStats.TurnoverMargin) * 0.3;

        var totalEdge = offensiveEdge + defensiveEdge + scoringEdge + 
                        scoringDefenseEdge + turnoverEdge;

        return totalEdge;
    }

    /// <summary>
    /// Calculates biorhythm-based advantage between two teams.
    /// Considers key players weighted by position importance and playing time.
    /// </summary>
    private double CalculateBiorhythmEdge(
        List<Player> homeRoster,
        List<Player> awayRoster,
        DateTime gameDate)
    {
        if (!homeRoster.Any() || !awayRoster.Any())
        {
            _logger.LogWarning("Missing roster data, returning neutral biorhythm edge");
            return 0.0;
        }

        var homeScore = CalculateTeamBiorhythmScore(homeRoster, gameDate);
        var awayScore = CalculateTeamBiorhythmScore(awayRoster, gameDate);

        // Scale the difference to be comparable to stats edge
        var biorhythmEdge = (homeScore - awayScore) * 5.0;

        return biorhythmEdge;
    }

    /// <summary>
    /// Calculates aggregate biorhythm score for a team's roster.
    /// </summary>
    private double CalculateTeamBiorhythmScore(List<Player> roster, DateTime gameDate)
    {
        var playerBiorhythms = new Dictionary<string, BiorhythmScore>();
        var playerWeights = new Dictionary<string, double>();

        foreach (var player in roster)
        {
            // Calculate biorhythm for each player
            var biorhythm = _biorhythmService.CalculateBiorhythm(
                player.DateOfBirth,
                gameDate);
            
            playerBiorhythms[player.PlayerId] = biorhythm;

            // Calculate player weight based on position importance and playing time
            var positionWeight = _config.PositionImportance.GetValueOrDefault(player.Position, 0.5);
            var playingTimeWeight = player.SnapsPerGame / 70.0; // Normalize by typical snaps
            var depthWeight = player.DepthChart.ToLower() == "starter" ? 1.0 : 0.5;
            
            playerWeights[player.PlayerId] = positionWeight * playingTimeWeight * depthWeight;
        }

        return _biorhythmService.CalculateTeamBiorhythm(playerBiorhythms, playerWeights);
    }

    /// <summary>
    /// Generates human-readable explanation of the prediction.
    /// </summary>
    private string GenerateExplanation(
        string homeTeamName,
        string awayTeamName,
        double statsEdge,
        double biorhythmEdge,
        double homeFieldAdvantage,
        string predictedWinner,
        double margin)
    {
        var explanation = $"{predictedWinner} is predicted to win by {margin:F1} points. ";

        if (Math.Abs(statsEdge) > 1.0)
        {
            var statsLeader = statsEdge > 0 ? homeTeamName : awayTeamName;
            explanation += $"{statsLeader} holds a statistical advantage ({Math.Abs(statsEdge):F1} pts). ";
        }

        if (Math.Abs(biorhythmEdge) > 0.5)
        {
            var bioLeader = biorhythmEdge > 0 ? homeTeamName : awayTeamName;
            explanation += $"{bioLeader}'s key players show favorable biorhythm patterns ({Math.Abs(biorhythmEdge):F1} pts). ";
        }

        explanation += $"Home field advantage adds {homeFieldAdvantage:F1} points for {homeTeamName}.";

        return explanation;
    }
}
