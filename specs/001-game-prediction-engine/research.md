# Research: NCAA Football Game Outcome Predictor - Implementation Choices

## Decision: Database for .NET/React POC
- **Chosen:** PostgreSQL
- **Rationale:**
  - Open source, scalable, and highly compatible with Docker
  - Excellent .NET support (Npgsql)
  - Cloud-ready, strong community, advanced features
- **Alternatives considered:** SQL Server (enterprise, heavier), SQLite (local dev only), MySQL/MariaDB (less feature-rich)

## Decision: React Unit Testing Framework
- **Chosen:** Jest + React Testing Library
- **Rationale:**
  - Industry standard for React/TypeScript
  - Maintains, user-focused, easy to set up, large community
- **Alternatives considered:** Vitest (Vite-native, fast), Cypress (E2E, not primary for unit tests)

## Decision: Devcontainer/Containerization for .NET + React Monorepo
- **Chosen:** Multi-stage Dockerfile with .NET 8 SDK and Node.js, orchestrated via docker-compose
- **Rationale:**
  - Supports both backend and frontend in one devcontainer
  - Enables hot-reload, VS Code integration, and DB service (PostgreSQL)
  - Follows best practices for security, developer experience, and extensibility
- **Alternatives considered:** Separate containers for each service (more complex for POC), single language images (less flexible)

---

**All major unknowns resolved. Ready for Phase 1: Design & Contracts.**

_Last updated: 2025-12-22_
