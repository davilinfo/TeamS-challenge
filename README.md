# Rock Paper Scissors Lizard Spock

A full-stack implementation of the extended RPSLS game — .NET 8 REST API + React UI.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Node.js 20+](https://nodejs.org/)
- (Optional) [Docker](https://www.docker.com/)

---

## Running Locally (no Docker)

### 1. Start the API

```bash
cd api
dotnet run --project RpsLs.Api
```

The API starts on **http://localhost:5000**. Swagger UI at **http://localhost:5000/swagger**.

Scoreboard data is stored in an **EF Core InMemory database** — no SQL Server or configuration needed.

### 2. Start the React UI

```bash
cd ui
npm install
npm run dev
```

Open **http://localhost:5173**.

---

## Running Tests

```bash
cd api
dotnet test
```

---

## Running with Docker

Starts the API + a SQL Server 2022 container. The API waits for SQL Server to be healthy, then auto-runs any pending EF Core migrations.

```bash
docker compose up --build
```

Open **http://localhost:5000**.

---

## API Endpoints

| Method | Path | Description |
|---|---|---|
| `GET` | `/choices` | All five choices |
| `GET` | `/choice` | Random computer choice |
| `POST` | `/play` | Play a round |
| `GET` | `/scoreboard` | Last 10 results |
| `DELETE` | `/scoreboard` | Reset scoreboard |

### POST /play — example

**Request:**
```json
{ "player": 1 }
```

**Response:**
```json
{ "results": "win", "player": 1, "computer": 4 }
```

---

## Project Structure

```
rpsls/
├── api/
│   ├── RpsLs.Api/               # ASP.NET Core 8 Web API + SPA host
│   │   ├── Controllers/         # GameController
│   │   ├── Models/              # Choice, PlayRequest, PlayResult, ScoreEntry (DTOs)
│   │   └── Program.cs           # DI setup, EF Core provider selection, CORS
│   ├── RpsLs.ApplicationService/ # Game logic, domain models, interfaces
│   │   ├── Interfaces/          # IGameService, IRandomService, IScoreRepository
│   │   ├── Models/              # Domain models
│   │   └── Services/            # GameService, RandomService
│   ├── RpsLs.Infra/             # Data access layer
│   │   ├── Data/                # AppDbContext, AppDbContextFactory
│   │   ├── Entities/            # ScoreRecord (EF Core entity)
│   │   ├── Migrations/          # EF Core migrations
│   │   └── Repositories/        # ScoreRepository
│   └── RpsLs.Tests/             # xUnit unit tests (Moq for mocking)
├── uix/                         # Vite + React + TypeScript + Redux Toolkit
│   └── src/
│       ├── components/          # ChoiceButton, ResultDisplay, Scoreboard
│       ├── slices/              # Redux state slices
│       ├── api.ts               # Typed fetch wrapper
│       ├── types.ts             # Shared TypeScript interfaces
│       └── App.tsx
├── Dockerfile                   # Multi-stage build (API + React → single image)
├── docker-compose.yml           # rpsls + SQL Server services
├── DECISIONS.md
└── README.md
```

---

## Persistence

| Environment | Storage |
|---|---|
| Local (no Docker) | EF Core InMemory — data lives for the process lifetime |
| Docker | SQL Server 2022 — data persists across restarts |

Detection uses the `RUNNING_IN_DOCKER=true` environment variable set in `docker-compose.yml`.

---

## EF Core migrations

```bash
cd api
dotnet ef migrations add <Name> \
  --project RpsLs.Infra \
  --startup-project RpsLs.Api
```

---

## AI Usage

Claude Code (claude-sonnet-4-6) was used to scaffold the project structure, generate boilerplate, and write initial implementations. Unit tests were developed and reviewed with xunit and moq, in memory db outside docker and sql server when on docker, the game logic dictionary (O(1) win lookup), the random-number mapping formula, CORS, Redux state management, EF Core repository pattern, and Docker build.
