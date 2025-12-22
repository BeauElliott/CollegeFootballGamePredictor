/**
 * API service layer for communicating with the backend prediction API.
 * Provides type-safe methods for fetching schedule data, making predictions,
 * and retrieving team information.
 */

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';

/**
 * Game entity from the backend API
 */
export interface Game {
  gameId: string;
  date: string;
  homeTeamId: string;
  awayTeamId: string;
  location: string;
  status: 'Scheduled' | 'InProgress' | 'Completed' | 'Postponed' | 'Cancelled';
  homeScore?: number;
  awayScore?: number;
  homeTeam?: Team;
  awayTeam?: Team;
}

/**
 * Team entity from the backend API
 */
export interface Team {
  teamId: string;
  name: string;
  conference: string;
}

/**
 * Team statistics from the backend API
 */
export interface TeamStats {
  teamId: string;
  season: number;
  ppg: number;
  totalOffenseRank: number;
  passingYardsRank: number;
  rushingYardsRank: number;
}

/**
 * Player entity from the backend API
 */
export interface Player {
  playerId: string;
  teamId: string;
  name: string;
  position: string;
  dateOfBirth: string;
}

/**
 * Prediction request payload
 */
export interface PredictionRequest {
  gameId: string;
}

/**
 * Breakdown component of a prediction
 */
export interface PredictionBreakdown {
  homeFieldAdvantage: number;
  statsEdge: number;
  biorhythmEdge: number;
}

/**
 * Prediction response from the backend API
 */
export interface PredictionResponse {
  gameId: string;
  predictedWinnerId: string;
  winProbability: number;
  margin: number;
  breakdown: PredictionBreakdown;
  timestamp: string;
}

/**
 * Generic error response from the API
 */
export interface ApiError {
  message: string;
  details?: string;
}

/**
 * Custom error class for API errors
 */
export class ApiException extends Error {
  constructor(
    message: string,
    public statusCode: number,
    public details?: string
  ) {
    super(message);
    this.name = 'ApiException';
  }
}

/**
 * Makes a typed HTTP request to the API
 */
async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const url = `${API_BASE_URL}${endpoint}`;
  
  const defaultOptions: RequestInit = {
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
    ...options,
  };

  try {
    const response = await fetch(url, defaultOptions);

    if (!response.ok) {
      let errorMessage = `HTTP ${response.status}: ${response.statusText}`;
      let errorDetails: string | undefined;

      try {
        const errorData = await response.json();
        errorMessage = errorData.message || errorMessage;
        errorDetails = errorData.details;
      } catch {
        // Response wasn't JSON, use status text
      }

      throw new ApiException(errorMessage, response.status, errorDetails);
    }

    return await response.json();
  } catch (error) {
    if (error instanceof ApiException) {
      throw error;
    }
    
    // Network error or other fetch failure
    throw new ApiException(
      'Network error: Unable to connect to the API',
      0,
      error instanceof Error ? error.message : String(error)
    );
  }
}

/**
 * API service methods
 */
export const api = {
  /**
   * Fetches all upcoming scheduled games
   */
  async getUpcomingGames(): Promise<Game[]> {
    return apiRequest<Game[]>('/api/schedule/upcoming');
  },

  /**
   * Fetches a specific game by ID
   */
  async getGame(gameId: string): Promise<Game> {
    return apiRequest<Game>(`/api/schedule/${gameId}`);
  },

  /**
   * Fetches all games, optionally filtered by team and/or status
   */
  async getSchedule(teamId?: string, status?: string): Promise<Game[]> {
    const params = new URLSearchParams();
    if (teamId) params.append('teamId', teamId);
    if (status) params.append('status', status);
    
    const query = params.toString();
    return apiRequest<Game[]>(`/api/schedule${query ? `?${query}` : ''}`);
  },

  /**
   * Fetches all teams, optionally filtered by conference
   */
  async getTeams(conference?: string): Promise<Team[]> {
    const params = conference ? `?conference=${conference}` : '';
    return apiRequest<Team[]>(`/api/teams${params}`);
  },

  /**
   * Fetches team statistics for a specific team
   */
  async getTeamStats(teamId: string): Promise<TeamStats[]> {
    return apiRequest<TeamStats[]>(`/api/teams/${teamId}/stats`);
  },

  /**
   * Fetches team roster for a specific team
   */
  async getTeamRoster(teamId: string, position?: string): Promise<Player[]> {
    const params = position ? `?position=${position}` : '';
    return apiRequest<Player[]>(`/api/teams/${teamId}/roster${params}`);
  },

  /**
   * Fetches prediction history for a specific game
   */
  async getGamePredictions(gameId: string): Promise<PredictionResponse[]> {
    return apiRequest<PredictionResponse[]>(`/api/schedule/${gameId}/predictions`);
  },

  /**
   * Requests a new prediction for a game
   */
  async predictGame(request: PredictionRequest): Promise<PredictionResponse> {
    return apiRequest<PredictionResponse>('/api/prediction', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  },

  /**
   * Checks if the API is healthy and responsive
   */
  async healthCheck(): Promise<{ status: string; timestamp: string; version: string }> {
    return apiRequest('/api/health');
  },
};

export default api;
