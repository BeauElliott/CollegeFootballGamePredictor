import api, { ApiException } from './api';

// Mock fetch globally
global.fetch = jest.fn();

describe('API Service', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('getUpcomingGames', () => {
    test('fetches upcoming games successfully', async () => {
      const mockGames = [
        { gameId: 'game-001', date: '2025-12-28T19:00:00Z', status: 'Scheduled' },
      ];

      (global.fetch as jest.Mock).mockResolvedValue({
        ok: true,
        json: async () => mockGames,
      });

      const result = await api.getUpcomingGames();

      expect(global.fetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/schedule/upcoming'),
        expect.objectContaining({
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
          }),
        })
      );
      expect(result).toEqual(mockGames);
    });
  });

  describe('getTeams', () => {
    test('fetches teams without filter', async () => {
      const mockTeams = [
        { teamId: 'alabama', name: 'Alabama', conference: 'SEC' },
      ];

      (global.fetch as jest.Mock).mockResolvedValue({
        ok: true,
        json: async () => mockTeams,
      });

      const result = await api.getTeams();

      expect(global.fetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/teams'),
        expect.any(Object)
      );
      expect(result).toEqual(mockTeams);
    });

    test('fetches teams with conference filter', async () => {
      const mockTeams = [
        { teamId: 'alabama', name: 'Alabama', conference: 'SEC' },
      ];

      (global.fetch as jest.Mock).mockResolvedValue({
        ok: true,
        json: async () => mockTeams,
      });

      await api.getTeams('SEC');

      expect(global.fetch).toHaveBeenCalledWith(
        expect.stringContaining('?conference=SEC'),
        expect.any(Object)
      );
    });
  });

  describe('predictGame', () => {
    test('submits prediction request successfully', async () => {
      const mockPrediction = {
        gameId: 'game-001',
        predictedWinnerId: 'alabama',
        winProbability: 0.68,
        margin: 7.5,
        breakdown: { homeFieldAdvantage: 3.0, statsEdge: 2.5, biorhythmEdge: 2.0 },
        timestamp: '2025-12-22T12:00:00Z',
      };

      (global.fetch as jest.Mock).mockResolvedValue({
        ok: true,
        json: async () => mockPrediction,
      });

      const result = await api.predictGame({ gameId: 'game-001' });

      expect(global.fetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/prediction'),
        expect.objectContaining({
          method: 'POST',
          body: JSON.stringify({ gameId: 'game-001' }),
        })
      );
      expect(result).toEqual(mockPrediction);
    });
  });

  describe('error handling', () => {
    test('throws ApiException on HTTP error', async () => {
      (global.fetch as jest.Mock).mockResolvedValue({
        ok: false,
        status: 404,
        statusText: 'Not Found',
        json: async () => ({ message: 'Game not found' }),
      });

      await expect(api.getGame('nonexistent')).rejects.toThrow(ApiException);
      await expect(api.getGame('nonexistent')).rejects.toThrow('Game not found');
    });

    test('throws ApiException on network error', async () => {
      (global.fetch as jest.Mock).mockRejectedValue(new Error('Network failure'));

      await expect(api.getUpcomingGames()).rejects.toThrow(ApiException);
      await expect(api.getUpcomingGames()).rejects.toThrow(/network error/i);
    });

    test('includes status code in ApiException', async () => {
      (global.fetch as jest.Mock).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: 'Internal Server Error',
        json: async () => ({}),
      });

      try {
        await api.getUpcomingGames();
        fail('Should have thrown ApiException');
      } catch (error) {
        expect(error).toBeInstanceOf(ApiException);
        if (error instanceof ApiException) {
          expect(error.statusCode).toBe(500);
        }
      }
    });
  });

  describe('healthCheck', () => {
    test('checks API health successfully', async () => {
      const mockHealth = {
        status: 'healthy',
        timestamp: '2025-12-22T12:00:00Z',
        version: '1.0.0',
      };

      (global.fetch as jest.Mock).mockResolvedValue({
        ok: true,
        json: async () => mockHealth,
      });

      const result = await api.healthCheck();

      expect(global.fetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/health'),
        expect.any(Object)
      );
      expect(result).toEqual(mockHealth);
    });
  });
});
