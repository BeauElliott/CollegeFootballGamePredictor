# Quick Start Guide - College Football Game Predictor

Get up and running in minutes with the College Football Game Predictor.

## Prerequisites

### Using Devcontainer (Recommended)
- **Docker Desktop** 4.20+
- **Visual Studio Code** 1.85+ with Remote Containers extension
- **Git** 2.40+

### Manual Setup
- **.NET 9 SDK** (https://dotnet.microsoft.com/download)
- **Node.js** 18+ LTS (https://nodejs.org/)
- **PostgreSQL** 16+ (or use Docker)
- **Docker** (for PostgreSQL container)

## Method 1: Devcontainer Setup (Recommended)

### 1. Clone and Open
```bash
git clone <repository-url>
cd CollegeFootballGamePredictor
code .
```

### 2. Reopen in Container
- VS Code will prompt "Reopen in Container" - click it
- Or: Command Palette (F1) → `Remote-Containers: Reopen in Container`
- Wait for build (3-5 minutes first time)

### 3. Start Database
```bash
docker-compose -f docker-compose.dev.yml up -d
```

### 4. Run Backend
```bash
cd backend
dotnet run --project src/Api
```
Backend: http://localhost:5000

### 5. Run Frontend (New Terminal)
```bash
cd frontend
npm start
```
Frontend: http://localhost:3000

### 6. Verify
- API Health: http://localhost:5000/api/health
- Swagger: http://localhost:5000/swagger
- Frontend: http://localhost:3000

## Method 2: Manual Setup

### 1. Clone Repository
```bash
git clone <repository-url>
cd CollegeFootballGamePredictor
```

### 2. Setup Database
**Docker (Recommended)**:
```bash
docker-compose -f docker-compose.dev.yml up -d
```

**Local PostgreSQL**:
```bash
psql -U postgres -c "CREATE DATABASE collegefootball;"
# Update connection string in backend/src/Api/appsettings.Development.json
```

### 3. Setup Backend
```bash
cd backend
dotnet restore
dotnet ef database update --project src/Api
dotnet run --project src/Api
```

### 4. Setup Frontend
```bash
cd frontend
npm install
npm start
```

## Running Tests

### Backend Tests
```bash
cd backend
dotnet test                    # All tests
dotnet test tests/unit         # Unit tests only
dotnet test tests/integration  # Integration tests only
```

**Results**: 41 unit tests (all passing), 22 integration tests (3 passing, 19 skipped)

### Frontend Tests
```bash
cd frontend
npm test                       # Interactive mode
npm test -- --watchAll=false   # Run once
```

**Results**: 26 tests (23 passing)

## Configuration

### API Keys (Optional)
For external data sources:
1. Get free key: https://collegefootballdata.com/key
2. Edit `backend/src/Api/appsettings.Development.json`:
```json
{
  "DataSources": {
    "CollegeFootballData": {
      "ApiKey": "your-api-key-here"
    }
  }
}
```

### Environment Variables
```bash
# Backend
export DataSources__CollegeFootballData__ApiKey="your-key"

# Frontend  
export REACT_APP_API_URL="http://localhost:5000"
```

## Common Commands

### Database
```bash
# Start/stop
docker-compose -f docker-compose.dev.yml up -d
docker-compose -f docker-compose.dev.yml down

# Connect
psql -h localhost -U cfbuser -d collegefootball
# Password: cfbpassword

# Migrations
cd backend
dotnet ef migrations add MigrationName --project src/Api
dotnet ef database update --project src/Api
```

### Backend
```bash
# Development
dotnet run --project src/Api
dotnet watch --project src/Api  # Hot reload

# Testing
dotnet test
dotnet test --filter "FullyQualifiedName~PredictionService"

# Build
dotnet build
dotnet build --configuration Release
```

### Frontend
```bash
# Development
npm start

# Testing
npm test
npm test -- --coverage

# Build
npm run build
```

## Troubleshooting

### Port Already in Use
```bash
# Kill process on port 5000 (backend)
lsof -ti:5000 | xargs kill -9

# Kill process on port 3000 (frontend)
lsof -ti:3000 | xargs kill -9
```

### Database Connection Failed
```bash
# Restart PostgreSQL
docker-compose -f docker-compose.dev.yml restart postgres

# Check logs
docker-compose -f docker-compose.dev.yml logs postgres
```

### Reset Database
```bash
docker-compose -f docker-compose.dev.yml down -v
docker-compose -f docker-compose.dev.yml up -d
cd backend
dotnet ef database update --project src/Api
```

### Frontend Dependencies
```bash
cd frontend
rm -rf node_modules package-lock.json
npm install
```

## Default Credentials

**Database**:
- Host: localhost:5432
- Database: collegefootball
- User: cfbuser
- Password: cfbpassword

## API Endpoints

Once running, explore the API at http://localhost:5000/swagger

**Key Endpoints**:
- `POST /api/prediction` - Generate prediction
- `GET /api/schedule/upcoming` - Upcoming games
- `GET /api/teams` - All teams
- `GET /api/config/active` - Active configuration
- `POST /api/admin/refresh/all` - Refresh all data

## Next Steps

1. **Make a Prediction**: Open http://localhost:3000, select a game, click "Get Prediction"
2. **Explore API**: Visit http://localhost:5000/swagger for interactive docs
3. **Run Tests**: Execute `dotnet test` and `npm test`
4. **Read Architecture**: See [spec.md](spec.md) for detailed design
5. **Review Code**: All code includes comprehensive JSDoc/XML comments

## Getting Help

- **Documentation**: `/docs` and `/specs` folders
- **Architecture**: [spec.md](spec.md)
- **Data Model**: [data-model.md](data-model.md)
- **Implementation**: [tasks.md](tasks.md)
- **API Docs**: http://localhost:5000/swagger (when running)

---

**Ready to develop!** 🚀

