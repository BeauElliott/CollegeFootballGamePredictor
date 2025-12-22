# NCAA Football Game Outcome Predictor – Requirements Specification

**Project Name:** NCAA Football Predictor  
**Technology Stack:** .NET 8 (or latest), C#, ASP.NET Core Web API (backend), optionally Blazor or minimal frontend for demo  
**Target Platform:** Web application (API-first, with optional simple UI)  
**Version:** 1.0  
**Date:** December 22, 2025  

## 1. Project Overview

The application predicts outcomes of NCAA FBS college football games, focusing on Power 4 conferences (ACC, Big Ten, Big 12, SEC) and College Football Playoff games.

The prediction model combines two primary components:
- Traditional team performance statistics (offense/defense efficiency, scoring, turnovers, etc.)
- An experimental player-level biorhythm scoring system based on date of birth, position importance, and estimated playing time/snaps

**Key Outputs:**
- Predicted winner
- Confidence level / win probability
- Optional point spread estimate
- Breakdown of contributing factors (team stats edge vs. roster/biorhythm edge)

## 2. Functional Requirements

### 2.1 Schedule Management
- **FR-01**: Retrieve and cache upcoming NCAA FBS football schedule
- **FR-02**: Filter schedule to include only Power 4 conference games and playoff/bowl games
- **FR-03**: Support manual refresh of schedule data
- **FR-04**: Expose endpoint to list upcoming games with date, teams, location, and game ID

### 2.2 Team Statistics Integration
- **FR-10**: Fetch and store current season team statistics including:
  - Offensive: PPG, total offense rank, passing yards/efficiency rank, rushing yards/efficiency rank, turnovers lost
  - Defensive: PPG allowed, total defense rank, passing yards allowed/efficiency rank, rushing yards allowed/efficiency rank, turnovers forced
  - Turnover margin
- **FR-11**: Normalize and rank teams across key statistical categories
- **FR-12**: Calculate team statistical edge for any head-to-head matchup

### 2.3 Roster and Player Data
- **FR-20**: Import and store roster data per team including:
  - Player name
  - Position (standardized: QB, RB, WR, TE, OT, OG, C, DT, EDGE, ILB, OLB, CB, S, etc.)
  - Date of birth (DOB)
  - Depth chart status (starter, backup, etc.) – if available
  - Optional: Average snaps per game or participation percentage
- **FR-21**: Support manual or automated update of roster data (CSV import or API scrape)

### 2.4 Biorhythm Calculation Engine
- **FR-30**: Implement standard biorhythm formula:
  - Physical cycle: 23 days
  - Emotional cycle: 28 days
  - Intellectual cycle: 33 days
  - Score per cycle: `sin(2π × days_since_birth / cycle_length) × 100`
- **FR-31**: Compute combined biorhythm score as weighted average (default: equal weights, configurable)
- **FR-32**: Calculate player contribution score = `biorhythm_score × position_importance × involvement_weight`

### 2.5 Position Importance Weights (Configurable)

| Position Group       | Importance Level | Default Weight |
|----------------------|------------------|----------------|
| QB                   | High             | 1.0            |
| WR, RT, LT           | Medium-High      | 0.9            |
| EDGE, ILB, OLB, CB   | Medium-High      | 0.9            |
| RB, TE               | Medium           | 0.6            |
| C, OG, RG            | Medium-Low       | 0.4            |
| DT, S                | Medium-Low       | 0.4            |

### 2.6 Prediction Engine
- **FR-40**: For any selected game, compute:
  - Team Stats Edge Score (weighted sum of normalized statistical advantages)
  - Roster/Biorhythm Edge Score (sum of player contributions for starters/key players)
  - Combined Prediction Score = `(w1 × Stats_Edge) + (w2 × Biorhythm_Edge) + home_field_adjustment`
- **FR-41**: Configurable weights (w1, w2) – default: 80% stats, 20% biorhythm
- **FR-42**: Home field advantage bonus (default +3 points or configurable)
- **FR-43**: Generate:
  - Predicted winner
  - Win probability (via logistic mapping of combined score)
  - Optional predicted margin (linear scaling of combined score)
  - Breakdown explanation (stats vs. biorhythm contribution)

### 2.7 API Endpoints (Core)
- `GET /api/schedule/upcoming` → List of upcoming games
- `GET /api/schedule/{gameId}` → Detailed game info
- `GET /api/teams` → List of teams (with basic info)
- `GET /api/teams/{teamId}/stats` → Current season stats
- `GET /api/teams/{teamId}/roster` → Player roster with biorhythm-ready data
- `POST /api/predict` → Input: gameId or teamA/teamB + date → Output: full prediction result

### 2.8 Admin / Data Management
- **FR-50**: Admin interface or endpoints to:
  - Trigger data refresh (schedule, stats, rosters)
  - Upload CSV for roster updates
  - Adjust model weights and position importance

## 3. Non-Functional Requirements

- **NFR-01**: Data should be cached and refreshed no more than once per day (or on demand)
- **NFR-02**: Prediction response time < 2 seconds
- **NFR-03**: Support for at least 134 FBS teams (full coverage preferred)
- **NFR-04**: Configurable via `appsettings.json` (weights, data sources, home field advantage)
- **NFR-05**: Logging of prediction requests and results for future backtesting
- **NFR-06**: Unit and integration test coverage for core calculation logic

## 4. Data Sources (External – To Be Integrated)

| Data Type              | Recommended Source                              | Update Frequency |
|------------------------|--------------------------------------------------|------------------|
| Schedule               | collegefootballdata.com API or NCAA.com          | Weekly           |
| Team Statistics        | NCAA official stats, TeamRankings.com, or CFD    | Weekly           |
| Rosters + DOB          | Sports-Reference, ESPN team pages, Ourlads       | Pre-season + updates |
| Depth Charts / Snaps   | PFF (paid), or approximate via starters          | As available     |

## 5. Future Enhancements (Out of Scope for v1)

- Machine learning model training on historical predictions
- Backtesting module against past seasons
- Public web UI with visualizations
- Recruiting rankings or talent composite integration
- Injury status integration
- Live in-game prediction updates

## 6. Suggested Project Structure (.NET)
