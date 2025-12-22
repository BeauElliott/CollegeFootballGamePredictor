# CollegeFootballGamePredictor

**NCAA Football Game Outcome Predictor** - Proof-of-Concept (POC)

A web application that predicts outcomes of NCAA FBS college football games, focusing on Power 4 conferences (ACC, Big Ten, Big 12, SEC) and College Football Playoff games. The prediction model combines traditional team performance statistics with an experimental player-level biorhythm scoring system.

## Project Structure

This is a multi-project monorepo with:
- **Backend**: .NET 8 C# (DataRetrieval, Processing, WebScraping, Api projects)
- **Frontend**: React with TypeScript
- **Database**: PostgreSQL (containerized)
- **Development**: Devcontainer for full encapsulation

### Backend Projects
- **DataRetrieval**: Entity Framework Core, data access, and retrieval
- **Processing**: Prediction logic, biorhythm calculations, and model processing
- **WebScraping**: External data source scraping (schedules, stats, rosters)
- **Api**: ASP.NET Core Web API endpoints

### Frontend
- **React**: Modern UI with TypeScript, Jest, and React Testing Library

## Quick Start

1. **Prerequisites**: Docker, .NET 9 SDK, Node.js (LTS)
2. **Devcontainer**: Open in VS Code and reopen in container
3. **Database**: Start PostgreSQL: `docker-compose -f docker-compose.dev.yml up -d`
4. **Backend**: `cd backend && dotnet run --project src/Api`
5. **Frontend**: `cd frontend && npm start`

See [specs/001-game-prediction-engine/quickstart.md](specs/001-game-prediction-engine/quickstart.md) for detailed setup.

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

- **Language/Version**: C# (.NET 9), JavaScript/TypeScript (React 19)
- **Primary Dependencies**: ASP.NET Core, Entity Framework Core, xUnit, PostgreSQL (Npgsql), React, Jest, React Testing Library
- **Storage**: PostgreSQL 16 (containerized, scalable, open source)
- **Testing**: xUnit + Moq + FluentAssertions (backend), Jest + React Testing Library (frontend)
- **Target Platform**: Docker containers, devcontainer, web
- **Constraints**: Maintainability, TDD, documentation, containerization

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
