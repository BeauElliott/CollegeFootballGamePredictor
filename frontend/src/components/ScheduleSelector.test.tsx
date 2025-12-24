import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import ScheduleSelector from './ScheduleSelector';
import api from '../services/api';

// Mock the API module
jest.mock('../services/api');

const mockGames = [
  {
    gameId: 'game-001',
    date: '2025-12-28T19:00:00Z',
    homeTeamId: 'alabama',
    awayTeamId: 'georgia',
    homeTeamName: 'Alabama Crimson Tide',
    awayTeamName: 'Georgia Bulldogs',
    location: 'Bryant-Denny Stadium',
    status: 'Scheduled' as const,
    homeTeam: { teamId: 'alabama', name: 'Alabama Crimson Tide', conference: 'SEC' },
    awayTeam: { teamId: 'georgia', name: 'Georgia Bulldogs', conference: 'SEC' },
  },
  {
    gameId: 'game-002',
    date: '2025-12-29T15:30:00Z',
    homeTeamId: 'ohio-state',
    awayTeamId: 'michigan',
    homeTeamName: 'Ohio State Buckeyes',
    awayTeamName: 'Michigan Wolverines',
    location: 'Ohio Stadium',
    status: 'Scheduled' as const,
    homeTeam: { teamId: 'ohio-state', name: 'Ohio State Buckeyes', conference: 'Big Ten' },
    awayTeam: { teamId: 'michigan', name: 'Michigan Wolverines', conference: 'Big Ten' },
  },
];

describe('ScheduleSelector', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders loading state initially', () => {
    (api.getUpcomingGames as jest.Mock).mockImplementation(
      () => new Promise(() => {}) // Never resolves
    );

    render(<ScheduleSelector onGameSelect={jest.fn()} />);
    
    expect(screen.getByText(/loading upcoming games/i)).toBeInTheDocument();
  });

  test('renders games after successful fetch', async () => {
    (api.getUpcomingGames as jest.Mock).mockResolvedValue(mockGames);

    render(<ScheduleSelector onGameSelect={jest.fn()} />);

    await waitFor(() => {
      expect(screen.getByText(/Alabama Crimson Tide/i)).toBeInTheDocument();
      expect(screen.getByText(/Georgia Bulldogs/i)).toBeInTheDocument();
      expect(screen.getByText(/Ohio State Buckeyes/i)).toBeInTheDocument();
      expect(screen.getByText(/Michigan Wolverines/i)).toBeInTheDocument();
    });
  });

  test('calls onGameSelect when game is clicked', async () => {
    (api.getUpcomingGames as jest.Mock).mockResolvedValue(mockGames);
    const mockOnGameSelect = jest.fn();

    render(<ScheduleSelector onGameSelect={mockOnGameSelect} />);

    await waitFor(() => {
      expect(screen.getByText(/Alabama Crimson Tide/i)).toBeInTheDocument();
    });

    const gameCard = screen.getByText(/Alabama Crimson Tide/i).closest('.game-card');
    expect(gameCard).toBeInTheDocument();
    
    if (gameCard) {
      await userEvent.click(gameCard);
      expect(mockOnGameSelect).toHaveBeenCalledWith(mockGames[0]);
    }
  });

  test('highlights selected game', async () => {
    (api.getUpcomingGames as jest.Mock).mockResolvedValue(mockGames);

    const { rerender } = render(
      <ScheduleSelector onGameSelect={jest.fn()} selectedGameId="game-001" />
    );

    await waitFor(() => {
      expect(screen.getByText(/Alabama Crimson Tide/i)).toBeInTheDocument();
    });

    const selectedCard = screen.getByText(/Alabama Crimson Tide/i).closest('.game-card');
    expect(selectedCard).toHaveClass('selected');

    rerender(<ScheduleSelector onGameSelect={jest.fn()} selectedGameId="game-002" />);

    const newSelectedCard = screen.getByText(/Ohio State Buckeyes/i).closest('.game-card');
    expect(newSelectedCard).toHaveClass('selected');
  });

  test('displays error message on fetch failure', async () => {
    (api.getUpcomingGames as jest.Mock).mockRejectedValue(
      new Error('Network error')
    );

    render(<ScheduleSelector onGameSelect={jest.fn()} />);

    await waitFor(() => {
      expect(screen.getByText(/error.*network error/i)).toBeInTheDocument();
    });
  });

  test('displays no games message when list is empty', async () => {
    (api.getUpcomingGames as jest.Mock).mockResolvedValue([]);

    render(<ScheduleSelector onGameSelect={jest.fn()} />);

    await waitFor(() => {
      expect(screen.getByText(/no upcoming games/i)).toBeInTheDocument();
    });
  });
});
