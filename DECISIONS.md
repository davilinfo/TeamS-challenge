# Technical Decisions

## Project Structure

The solution is split into four .NET projects with clear responsibilities:

- **RpsLs.Api** — HTTP layer only (controllers, DI wiring, middleware)
- **RpsLs.ApplicationService** — game logic, domain models, interfaces (`IGameService`, `IRandomService`, `IScoreRepository`)
- **RpsLs.Infra** — data access: EF Core `AppDbContext`, `ScoreRecord` entity, `ScoreRepository`
- **RpsLs.Tests** — unit tests

This keeps the game logic independent of HTTP and database concerns. Swapping persistence or transport doesn't touch `ApplicationService`.

## Game Logic: Dictionary<int, HashSet<int>>

Win conditions are expressed as a `Dictionary<int, HashSet<int>>` where each key maps to the choices it defeats. This gives O(1) lookup and is easy to extend. A 5×5 matrix or chain of if/else would work but adds noise.

## Random Number Mapping

The external service returns 1–100. Mapping to a choice uses `(number - 1) % 5`, giving even distribution across IDs 1–5. Therefore modulo is cleaner.

## Resilience for the External Random Service

`RandomService` falls back to `Random.Shared` if the external endpoint fails, logged at `Warning` level. This keeps the game playable in dev or when the third-party service is down.

## Scoreboard Persistence

Scoreboard data is persisted via `IScoreRepository` / `ScoreRepository` backed by EF Core:

- **Outside Docker** — `UseInMemoryDatabase`: no dependencies, zero config, data lives for the process lifetime.
- **Inside Docker** — `UseSqlServer`: full persistence across restarts, migrations applied automatically on startup.

Provider selection is driven by the `RUNNING_IN_DOCKER` environment variable injected by `docker-compose.yml`.

## Repository Pattern

`IScoreRepository` lives in `ApplicationService` so `GameService` depends only on an abstraction.

## Scoped Lifetime

`GameService` is registered as **Scoped** (one per request).

## Controllers vs Minimal API

MVC controller style (`[ApiController]`) was chosen over minimal APIs for free model validation via `[Range]` attributes, cleaner Swagger output, and broader team familiarity.

## CORS

Allowed origins are read from `appsettings.json` (`AllowedOrigins`) rather than hardcoded, so different environments can configure CORS without a recompile.

## React UI: Vite + Redux Toolkit

The frontend uses Vite + React + TypeScript. Redux Toolkit manages shared client state (choices, results, scoreboard). No UI component library was added — the game is simple then plain CSS produces a cleaner, faster result.

## Docker

Multi-stage Dockerfile: one stage compiles the React app, another publishes the .NET API, and the runtime image is the lean `aspnet:8.0`. React assets are copied into `wwwroot` and served as static files, shipping the full stack as a single container on port 5000.

`docker-compose.yml` adds a SQL Server 2022 service with a healthcheck. The API container uses `depends_on: condition: service_healthy` to wait for SQL Server before starting, then auto-applies migrations via `db.Database.Migrate()`.

## Testing

Unit tests cover all win/lose/tie permutations, random-number mapping, invalid input handling, and scoreboard delegation, also controllers and repository.