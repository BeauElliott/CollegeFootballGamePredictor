import React from 'react';
import { render, screen } from '@testing-library/react';
import App from './App';
import api from './services/api';

// Mock the API module
jest.mock('./services/api');

describe('App', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    // Mock API calls to prevent actual network requests
    (api.getUpcomingGames as jest.Mock).mockResolvedValue([]);
  });

  test('renders app title', () => {
    render(<App />);
    const titleElement = screen.getByText(/college football game predictor/i);
    expect(titleElement).toBeInTheDocument();
  });

  test('renders app subtitle', () => {
    render(<App />);
    const subtitleElement = screen.getByText(/advanced predictions.*statistics.*biorhythm/i);
    expect(subtitleElement).toBeInTheDocument();
  });

  test('renders both ScheduleSelector and PredictionForm sections', () => {
    render(<App />);
    
    expect(screen.getByText(/select a game/i)).toBeInTheDocument();
    expect(screen.getByText(/please select a game to predict/i)).toBeInTheDocument();
  });

  test('renders footer', () => {
    render(<App />);
    
    const footerElement = screen.getByText(/© 2025 college football game predictor/i);
    expect(footerElement).toBeInTheDocument();
  });
});

