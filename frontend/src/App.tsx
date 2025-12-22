import React, { useState } from 'react';
import ScheduleSelector from './components/ScheduleSelector';
import PredictionForm from './components/PredictionForm';
import { Game } from './services/api';
import './App.css';

/**
 * Main App component for the College Football Game Predictor.
 * 
 * This component orchestrates the prediction workflow:
 * 1. User selects a game from the schedule
 * 2. User requests a prediction
 * 3. Prediction results are displayed with detailed breakdown
 * 
 * The app features a modern blue/green color scheme with
 * responsive design for mobile and desktop devices.
 */
function App() {
  const [selectedGame, setSelectedGame] = useState<Game | null>(null);

  /**
   * Handles game selection from the ScheduleSelector
   */
  const handleGameSelect = (game: Game) => {
    setSelectedGame(game);
  };

  return (
    <div className="App">
      <header className="App-header">
        <div className="header-content">
          <h1 className="app-title">College Football Game Predictor</h1>
          <p className="app-subtitle">
            Advanced predictions powered by statistics and biorhythm analysis
          </p>
        </div>
      </header>

      <main className="App-main">
        <div className="content-grid">
          <section className="schedule-section">
            <ScheduleSelector
              onGameSelect={handleGameSelect}
              selectedGameId={selectedGame?.gameId}
            />
          </section>

          <section className="prediction-section">
            <PredictionForm game={selectedGame} />
          </section>
        </div>
      </main>

      <footer className="App-footer">
        <p>&copy; 2025 College Football Game Predictor</p>
      </footer>
    </div>
  );
}

export default App;

