import React, { useState } from 'react';
import api, { Game, PredictionResponse } from '../services/api';
import './PredictionForm.css';

/**
 * Props for the PredictionForm component
 */
interface PredictionFormProps {
  /** The game to predict */
  game: Game | null;
}

/**
 * PredictionForm component handles prediction requests and displays results.
 * 
 * Features:
 * - Displays selected game information
 * - Submits prediction requests to the API
 * - Shows prediction results with win probability and margin
 * - Displays detailed breakdown of prediction factors
 * - Handles loading and error states
 */
const PredictionForm: React.FC<PredictionFormProps> = ({ game }) => {
  const [prediction, setPrediction] = useState<PredictionResponse | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  /**
   * Handles prediction request submission
   */
  const handlePredict = async () => {
    if (!game) return;

    try {
      setLoading(true);
      setError(null);
      setPrediction(null);

      const result = await api.predictGame({ gameId: game.gameId });
      setPrediction(result);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Failed to generate prediction'
      );
    } finally {
      setLoading(false);
    }
  };

  /**
   * Gets the predicted winner's team name
   */
  const getPredictedWinnerName = (): string => {
    if (!prediction || !game) return '';
    
    if (prediction.predictedWinnerId === game.homeTeamId) {
      return game.homeTeamName;
    }
    return game.awayTeamName;
  };

  /**
   * Formats a percentage from a decimal probability
   */
  const formatPercentage = (value: number): string => {
    return `${(value * 100).toFixed(1)}%`;
  };

  /**
   * Formats the prediction breakdown values
   */
  const formatBreakdownValue = (value: number): string => {
    return value >= 0 ? `+${value.toFixed(2)}` : value.toFixed(2);
  };

  if (!game) {
    return (
      <div className="prediction-form">
        <div className="no-game-selected">
          <p>Please select a game to predict</p>
        </div>
      </div>
    );
  }

  return (
    <div className="prediction-form">
      <div className="game-info">
        <h2>Game Prediction</h2>
        <div className="matchup">
          <div className="team-info">
            <span className="team-label">Away</span>
            <span className="team-name">{game.awayTeamName}</span>
          </div>
          <div className="at">@</div>
          <div className="team-info">
            <span className="team-label">Home</span>
            <span className="team-name">{game.homeTeamName}</span>
          </div>
        </div>
        <div className="game-meta">
          <span>{new Date(game.date).toLocaleDateString('en-US', {
            weekday: 'long',
            month: 'long',
            day: 'numeric',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
          })}</span>
          <span className="separator">•</span>
          <span>{game.location}</span>
        </div>
      </div>

      <button
        className="predict-button"
        onClick={handlePredict}
        disabled={loading}
      >
        {loading ? 'Generating Prediction...' : 'Get Prediction'}
      </button>

      {error && (
        <div className="error-message">
          <p>Error: {error}</p>
        </div>
      )}

      {prediction && (
        <div className="prediction-result">
          <div className="result-header">
            <h3>Prediction Result</h3>
            <div className="timestamp">
              {new Date(prediction.timestamp).toLocaleString()}
            </div>
          </div>

          <div className="winner-prediction">
            <div className="winner-label">Predicted Winner</div>
            <div className="winner-name">{getPredictedWinnerName()}</div>
            <div className="win-probability">
              {formatPercentage(prediction.winProbability)} win probability
            </div>
            <div className="margin">
              Margin: {Math.abs(prediction.margin).toFixed(1)} points
            </div>
          </div>

          <div className="prediction-breakdown">
            <h4>Prediction Breakdown</h4>
            <div className="breakdown-items">
              <div className="breakdown-item">
                <span className="breakdown-label">Home Field Advantage</span>
                <span className="breakdown-value">
                  {formatBreakdownValue(prediction.breakdown.homeFieldAdvantage)}
                </span>
              </div>
              <div className="breakdown-item">
                <span className="breakdown-label">Statistics Edge</span>
                <span className="breakdown-value">
                  {formatBreakdownValue(prediction.breakdown.statsEdge)}
                </span>
              </div>
              <div className="breakdown-item">
                <span className="breakdown-label">Biorhythm Edge</span>
                <span className="breakdown-value">
                  {formatBreakdownValue(prediction.breakdown.biorhythmEdge)}
                </span>
              </div>
            </div>
          </div>

          <div className="disclaimer">
            <p>
              This prediction is based on statistical analysis, team performance data,
              and biorhythm calculations. Actual game outcomes may vary.
            </p>
          </div>
        </div>
      )}
    </div>
  );
};

export default PredictionForm;
