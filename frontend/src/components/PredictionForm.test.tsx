import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import PredictionForm from './PredictionForm';
import api from '../services/api';

// Mock the API module
jest.mock('../services/api');

const mockGame = {
  gameId: 'game-001',
  date: '2025-12-28T19:00:00Z',
  homeTeamId: 'alabama',
  homeTeamName: 'Alabama Crimson Tide',
  awayTeamId: 'georgia',
  awayTeamName: 'Georgia Bulldogs',
  location: 'Bryant-Denny Stadium',
  status: 'Scheduled' as const,
  homeTeam: { teamId: 'alabama', name: 'Alabama Crimson Tide', conference: 'SEC' },
  awayTeam: { teamId: 'georgia', name: 'Georgia Bulldogs', conference: 'SEC' },
};

const mockPrediction = {
  gameId: 'game-001',
  predictedWinnerId: 'alabama',
  winProbability: 0.68,
  margin: 7.5,
  breakdown: {
    homeFieldAdvantage: 3.0,
    statsEdge: 2.5,
    biorhythmEdge: 2.0,
  },
  timestamp: '2025-12-22T12:00:00Z',
};

describe('PredictionForm', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('displays message when no game is selected', () => {
    render(<PredictionForm game={null} />);
    
    expect(screen.getByText(/please select a game/i)).toBeInTheDocument();
  });

  test('displays game information when game is selected', () => {
    render(<PredictionForm game={mockGame} />);
    
    expect(screen.getByText(/Alabama Crimson Tide/i)).toBeInTheDocument();
    expect(screen.getByText(/Georgia Bulldogs/i)).toBeInTheDocument();
    expect(screen.getByText(/Bryant-Denny Stadium/i)).toBeInTheDocument();
  });

  test('shows predict button when game is selected', () => {
    render(<PredictionForm game={mockGame} />);
    
    const button = screen.getByRole('button', { name: /get prediction/i });
    expect(button).toBeInTheDocument();
    expect(button).not.toBeDisabled();
  });

  test('calls API and displays prediction on button click', async () => {
    (api.predictGame as jest.Mock).mockResolvedValue(mockPrediction);

    render(<PredictionForm game={mockGame} />);
    
    const button = screen.getByRole('button', { name: /get prediction/i });
    await userEvent.click(button);

    await waitFor(() => {
      expect(api.predictGame).toHaveBeenCalledWith({ gameId: 'game-001' });
    });

    await waitFor(() => {
      expect(screen.getByText(/predicted winner/i)).toBeInTheDocument();
      expect(screen.getByText(/68\.0% win probability/i)).toBeInTheDocument();
      expect(screen.getByText(/margin.*7\.5 points/i)).toBeInTheDocument();
      // Check for winner specifically in the winner section
      expect(screen.getByRole('heading', { level: 3 })).toHaveTextContent('Prediction Result');
    });
  });

  test('displays prediction breakdown', async () => {
    (api.predictGame as jest.Mock).mockResolvedValue(mockPrediction);

    render(<PredictionForm game={mockGame} />);
    
    const button = screen.getByRole('button', { name: /get prediction/i });
    await userEvent.click(button);

    await waitFor(() => {
      expect(screen.getByText(/home field advantage/i)).toBeInTheDocument();
      expect(screen.getByText(/\+3\.00/)).toBeInTheDocument();
      expect(screen.getByText(/statistics edge/i)).toBeInTheDocument();
      expect(screen.getByText(/\+2\.50/)).toBeInTheDocument();
      expect(screen.getByText(/biorhythm edge/i)).toBeInTheDocument();
      expect(screen.getByText(/\+2\.00/)).toBeInTheDocument();
    });
  });

  test('displays loading state while fetching prediction', async () => {
    (api.predictGame as jest.Mock).mockImplementation(
      () => new Promise((resolve) => setTimeout(() => resolve(mockPrediction), 100))
    );

    render(<PredictionForm game={mockGame} />);
    
    const button = screen.getByRole('button', { name: /get prediction/i });
    await userEvent.click(button);

    expect(screen.getByText(/generating prediction/i)).toBeInTheDocument();
    expect(button).toBeDisabled();

    await waitFor(() => {
      expect(screen.getByText(/predicted winner/i)).toBeInTheDocument();
    });
  });

  test('displays error message on API failure', async () => {
    (api.predictGame as jest.Mock).mockRejectedValue(
      new Error('Prediction failed')
    );

    render(<PredictionForm game={mockGame} />);
    
    const button = screen.getByRole('button', { name: /get prediction/i });
    await userEvent.click(button);

    await waitFor(() => {
      expect(screen.getByText(/error.*prediction failed/i)).toBeInTheDocument();
    });
  });

  test('clears previous prediction when requesting new one', async () => {
    (api.predictGame as jest.Mock).mockResolvedValue(mockPrediction);

    render(<PredictionForm game={mockGame} />);
    
    const button = screen.getByRole('button', { name: /get prediction/i });
    
    // First prediction
    await userEvent.click(button);
    await waitFor(() => {
      expect(screen.getByText(/predicted winner/i)).toBeInTheDocument();
    });

    // Second prediction - should clear first
    const newPrediction = { ...mockPrediction, predictedWinnerId: 'georgia', winProbability: 0.55 };
    (api.predictGame as jest.Mock).mockResolvedValue(newPrediction);
    
    await userEvent.click(button);
    
    // Should show new prediction
    await waitFor(() => {
      expect(screen.getByText(/55\.0% win probability/i)).toBeInTheDocument();
    });
  });
});
