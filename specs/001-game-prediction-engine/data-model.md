# Data Model: NCAA Football Game Outcome Predictor

## Entities

### Game
- gameId: string
- date: DateTime
- teams: [Team]
- location: string
- status: enum (Scheduled, Completed, Canceled, Postponed)

### Team
- teamId: string
- name: string
- conference: string
- stats: TeamStats
- roster: [Player]

### TeamStats
- season: int
- ppg: float
- totalOffenseRank: int
- passingYardsRank: int
- rushingYardsRank: int
- turnoversLost: int
- ppgAllowed: float
- totalDefenseRank: int
- passingYardsAllowedRank: int
- rushingYardsAllowedRank: int
- turnoversForced: int
- turnoverMargin: float

### Player
- playerId: string
- name: string
- position: string
- dob: Date
- depthChart: string (Starter, Backup, etc.)
- snapsPerGame: float

### Prediction
- gameId: string
- predictedWinner: string
- winProbability: float
- margin: float
- breakdown: PredictionBreakdown

### PredictionBreakdown
- statsEdge: float
- biorhythmEdge: float
- homeFieldAdjustment: float
- explanation: string

---

## Relationships
- Game has two Teams
- Team has many Players
- Prediction is for a Game

## Validation Rules
- All IDs must be unique
- Player DOB must be a valid date
- Stats must be non-negative
- Probabilities must be between 0 and 1
- Margin can be positive or negative

## State Transitions
- Game: Scheduled → Completed | Canceled | Postponed
- Prediction: Created per game, updated if data changes

_Last updated: 2025-12-22_
