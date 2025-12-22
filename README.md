# CollegeFootballGamePredictor

**NCAA Football Game Outcome Predictor** - Full-Stack Web Application

A modern web application that predicts outcomes of NCAA FBS college football games using statistical analysis and experimental biorhythm calculations. The system combines traditional team performance metrics with player-level biorhythm scoring to generate predictions with detailed breakdowns.

## 🎯 Features

- **Game Predictions**: Advanced ML-style predictions combining statistics and biorhythm analysis
- **Schedule Browsing**: View upcoming games with team information
- **Team Statistics**: Access detailed team performance metrics
- **Prediction History**: Track prediction accuracy over time
- **Admin Controls**: Data refresh and configuration management
- **Modern UI**: Responsive React frontend with blue/green theme

## 🏗️ Project Structure

This is a multi-project monorepo with:
- **Backend**: .NET 9 C# (DataRetrieval, Processing, WebScraping, Api projects)
- **Frontend**: React 19 with TypeScript
- **Database**: PostgreSQL 16 (containerized)
- **Development**: Devcontainer for full encapsulation

### Backend Projects (.NET 9)
- **DataRetrieval**: Entity Framework Core, data access, entities, and database context
- **Processing**: Prediction engine, biorhythm calculations, configuration management
- **WebScraping**: External data source integration (schedules, stats, rosters)
- **Api**: ASP.NET Core Web API with 5 controllers and 20+ endpoints

### Frontend (React 19)
- **React**: Modern UI with TypeScript, hooks, comprehensive testing
- **Components**: ScheduleSelector, PredictionForm with responsive design
- **API Layer**: Type-safe service layer with full error handling
- **Testing**: 26 tests with Jest and React Testing Library

### Testing
- **Backend**: 41 unit tests, 22 integration tests (xUnit, Moq, FluentAssertions)
- **Frontend**: 26 component and integration tests (Jest, React Testing Library)
- **Coverage**: All major features and error paths

## 🚀 Quick Start

## 🚀 Quick Start

### Using Devcontainer (Recommended)

1. **Prerequisites**: Docker Desktop, VS Code with Remote Containers extension
2. **Open in Container**: Open folder in VS Code, click "Reopen in Container"
3. **Wait for Setup**: Container will build and install all dependencies
4. **Start Database**: `docker-compose -f docker-compose.dev.yml up -d`
5. **Run Backend**: `cd backend && dotnet run --project src/Api`
6. **Run Frontend**: In new terminal: `cd frontend && npm start`
7. **Access App**: Frontend at http://localhost:3000, API at http://localhost:5000

### Manual Setup

1. **Prerequisites**: .NET 9 SDK, Node.js 18+, PostgreSQL 16, Docker
2. **Clone Repository**: `git clone <repo-url> && cd CollegeFootballGamePredictor`
3. **Database**: Start PostgreSQL container: `docker-compose -f docker-compose.dev.yml up -d`
4. **Backend Setup**:
   ```bash
   cd backend
   dotnet restore
   dotnet ef database update --project src/Api
   dotnet run --project src/Api
   ```
5. **Frontend Setup**:
   ```bash
   cd frontend
   npm install
   npm start
   ```
6. **Configure API Key**: See Configuration section below

See [specs/001-game-prediction-engine/quickstart.md](specs/001-game-prediction-engine/quickstart.md) for detailed setup.

## 📡 API Endpoints

### Prediction API
- `POST /api/prediction` - Generate game prediction
- `GET /api/prediction/{id}` - Get prediction by ID

### Core Data API  
- `GET /api/schedule/upcoming` - Upcoming scheduled games
- `GET /api/schedule/{gameId}` - Specific game details
- `GET /api/schedule` - All games with filters
- `GET /api/teams` - All teams
- `GET /api/teams/{teamId}/stats` - Team statistics
- `GET /api/teams/{teamId}/roster` - Team roster
- `GET /api/schedule/{gameId}/predictions` - Prediction history

### Admin API
- `POST /api/admin/refresh/teams` - Refresh team data
- `POST /api/admin/refresh/schedule` - Refresh schedule
- `POST /api/admin/refresh/stats` - Refresh statistics
- `POST /api/admin/refresh/all` - Refresh all data

### Configuration API
- `GET /api/config` - All configurations
- `GET /api/config/active` - Active configuration
- `PUT /api/config` - Update active configuration
- `POST /api/config/{id}/activate` - Activate configuration
- `DELETE /api/config/{id}` - Delete configuration

Full API documentation: [OpenAPI Specification](specs/001-game-prediction-engine/contracts/openapi.yaml)


### Configuration

#### Data Sources

The application integrates with multiple data sources for game, team, and player information. Configure these in [backend/src/Api/appsettings.json](backend/src/Api/appsettings.json):

- **College Football Data API** (https://collegefootballdata.com)
  - Primary source for schedules, stats, and rosters
  - Requires free API key from https://collegefootballdata.com/key
  - Rate limit: 200 requests/minute
  
- **ESPN API** (https://site.api.espn.com)
  - Supplementary data for games and teams
  - No API key required
  - Rate limit: 100 requests/minute
  
- **Sports Reference** (web scraping)
  - Fallback for historical data
  - Use responsibly with rate limiting
  - Disabled by default

Set your API keys in `appsettings.Development.json` or via environment variables:
```json
{
  "DataSources": {
    "CollegeFootballData": {
      "ApiKey": "ug+cNpQsfkVMJr2iDb/HLqkt1vjU7CbC1EmDkhppDBENSo7DlG+7VvxLh0NpHoyc"
    }
  }
}
```

Or use environment variables:
```bash
export DataSources__CollegeFootballData__ApiKey="ug+cNpQsfkVMJr2iDb/HLqkt1vjU7CbC1EmDkhppDBENSo7DlG+7VvxLh0NpHoyc"
```


## Documentation

- **Requirements**: [docs/requirements.md](docs/requirements.md)
- **Specification**: [specs/001-game-prediction-engine/spec.md](specs/001-game-prediction-engine/spec.md)
- **Implementation Plan**: [specs/001-game-prediction-engine/plan.md](specs/001-game-prediction-engine/plan.md)
- **Tasks**: [specs/001-game-prediction-engine/tasks.md](specs/001-game-prediction-engine/tasks.md)
- **Data Model**: [specs/001-game-prediction-engine/data-model.md](specs/001-game-prediction-engine/data-model.md)
- **API Contracts**: [specs/001-game-prediction-engine/contracts/openapi.yaml](specs/001-game-prediction-engine/contracts/openapi.yaml)

## Constitution & Best Practices

This project follows strict principles defined in [.specify/memory/constitution.md](.specify/memory/constitution.md):
- **Test-Driven Development (TDD)**: All code is test-first
- **Specification as Authority**: Implementation follows approved specs
- **Clean Code**: Clear naming, small functions, documented
- **Explicit State**: No hidden state, transparent transitions
- **Observability**: Logging, error reporting, traceability
- **Documentation & Maintainability**: Thoroughly documented, maintainability prioritized

See [.specify/compliance-checklist.md](.specify/compliance-checklist.md) for compliance review.

## Technology Stack

- **Backend**: C# .NET 9, ASP.NET Core, Entity Framework Core 9.0
- **Frontend**: React 19, TypeScript 4.9, Modern Hooks API
- **Database**: PostgreSQL 16 with Npgsql 9.0
- **Testing**: xUnit 2.9 (backend), Jest + React Testing Library (frontend)
- **Dependencies**: Moq 4.20, FluentAssertions 6.12 (backend), @testing-library/react 16.3 (frontend)
- **Platform**: Docker containers, Linux (Debian), devcontainer
- **Tooling**: .NET CLI, npm, EF Core migrations

## 📊 Implementation Status

### ✅ Completed Phases

**Phase 1-2: Foundation** (T001-T013)
- Project structure and devcontainer setup
- PostgreSQL database with EF Core
- Entity models and migrations
- Testing infrastructure (unit + integration)
- Middleware (error handling, logging)

**Phase 3: User Story 1 - Prediction MVP** (T014-T021)
- Biorhythm calculation service
- Prediction engine with configurable weights
- POST /api/prediction endpoint
- 10 comprehensive tests (all passing)

**Phase 4: User Story 2 - Data Management** (T022-T026)
- External data service integration
- Data refresh orchestration
- 5 admin endpoints for data management
- 17 tests (12 passing, 5 skipped)

**Phase 5: User Story 3 - Configuration** (T027-T032)
- Configuration service with versioning and caching
- 6 configuration management endpoints
- Prediction configuration with position importance
- 28 tests (15 passing, 13 skipped)

**Phase 6: User Story 4 - Core Data API** (T033-T036)
- 7 GET endpoints for schedule, teams, stats, roster
- CoreDataController with comprehensive error handling
- 41 unit tests (all passing), 22 integration tests

**Phase 7: Frontend MVP** (T037-T042)
- React application with ScheduleSelector and PredictionForm
- Type-safe API service layer
- Blue/green responsive design
- 26 frontend tests (23 passing)

**Phase 8: Polish & Cross-Cutting** (In Progress)
- Project documentation and guides
- CI/CD pipeline setup
- Code quality tooling
- Compliance review

## Development Workflow

1. All work begins with an approved specification
2. Tests are written before implementation (TDD)
3. Code is implemented to pass all tests and meet the specification
4. Code reviews check for compliance with all principles
5. Documentation is updated with every change
6. No code is merged without passing all tests and review gates

## License

This is a proof-of-concept project. See project documentation for licensing details.

---

_Last updated: 2025-12-22_
