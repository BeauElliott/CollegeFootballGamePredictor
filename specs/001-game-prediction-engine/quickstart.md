# Quickstart: NCAA Football Game Outcome Predictor

## Prerequisites
- Docker & Docker Compose
- .NET 8 SDK
- Node.js (LTS)
- PostgreSQL (or use Docker service)

## Getting Started

1. **Clone the repository**
2. **Start devcontainer** (VS Code):
   - Open folder in VS Code
   - Reopen in container (auto-builds backend, frontend, DB)
3. **Restore dependencies**
   - Backend: `dotnet restore ./backend`
   - Frontend: `cd frontend && yarn install`
4. **Run database migrations**
   - Backend: `dotnet ef database update` (if using EF Core)
5. **Start services**
   - Backend: `dotnet run --project ./backend/src/Api`
   - Frontend: `cd frontend && yarn start`
6. **Access app**
   - API: http://localhost:5000
   - Web: http://localhost:3000

## Running Tests
- Backend: `dotnet test ./backend/tests`
- Frontend: `cd frontend && yarn test`

## Configuration
- Edit `appsettings.json` for backend config (DB, weights, etc.)
- Edit `.env` for frontend config

---

For more, see README.md and docs/requirements.md

_Last updated: 2025-12-22_
