import React, { useEffect, useState } from 'react';
import api, { Game } from '../services/api';
import './ScheduleSelector.css';

/**
 * Props for the ScheduleSelector component
 */
interface ScheduleSelectorProps {
  /** Callback when a game is selected */
  onGameSelect: (game: Game) => void;
  /** Currently selected game ID */
  selectedGameId?: string;
}

/**
 * ScheduleSelector component displays a list of upcoming games
 * and allows the user to select one for prediction.
 * 
 * Features:
 * - Fetches upcoming scheduled games from the API
 * - Displays games with team names, date, and location
 * - Highlights the currently selected game
 * - Shows loading and error states
 */
const ScheduleSelector: React.FC<ScheduleSelectorProps> = ({
  onGameSelect,
  selectedGameId,
}) => {
  const [games, setGames] = useState<Game[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  /**
   * Fetches upcoming games when component mounts
   */
  useEffect(() => {
    const fetchGames = async () => {
      try {
        setLoading(true);
        setError(null);
        const upcomingGames = await api.getUpcomingGames();
        setGames(upcomingGames);
      } catch (err) {
        setError(
          err instanceof Error ? err.message : 'Failed to load games'
        );
      } finally {
        setLoading(false);
      }
    };

    fetchGames();
  }, []);

  /**
   * Formats a date string into a readable format
   */
  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      weekday: 'short',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (loading) {
    return (
      <div className="schedule-selector">
        <h2>Select a Game</h2>
        <div className="loading">Loading upcoming games...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="schedule-selector">
        <h2>Select a Game</h2>
        <div className="error">
          <p>Error: {error}</p>
          <button onClick={() => window.location.reload()}>Retry</button>
        </div>
      </div>
    );
  }

  if (games.length === 0) {
    return (
      <div className="schedule-selector">
        <h2>Select a Game</h2>
        <div className="no-games">
          <p>No upcoming games scheduled.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="schedule-selector">
      <h2>Select a Game</h2>
      <div className="games-list">
        {games.map((game) => (
          <div
            key={game.gameId}
            className={`game-card ${
              selectedGameId === game.gameId ? 'selected' : ''
            }`}
            onClick={() => onGameSelect(game)}
            role="button"
            tabIndex={0}
            onKeyPress={(e) => {
              if (e.key === 'Enter' || e.key === ' ') {
                onGameSelect(game);
              }
            }}
          >
            <div className="game-teams">
              <div className="team away-team">
                <span className="team-label">Away</span>
                <span className="team-name">
                  {game.awayTeamName || game.awayTeamId}
                </span>
              </div>
              <div className="vs">@</div>
              <div className="team home-team">
                <span className="team-label">Home</span>
                <span className="team-name">
                  {game.homeTeamName || game.homeTeamId}
                </span>
              </div>
            </div>
            <div className="game-details">
              <div className="game-date">{formatDate(game.date)}</div>
              <div className="game-location">{game.location}</div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default ScheduleSelector;
