# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Build
dotnet build

# Run API (auto-migrates DB and seeds data on startup)
dotnet run --project src/FitTrackPro.API

# Run all tests
dotnet test

# Run a specific test project
dotnet test tests/FitTrackPro.Application.Tests
dotnet test tests/FitTrackPro.API.IntegrationTests
dotnet test tests/FitTrackPro.Domain.Tests
dotnet test tests/FitTrackPro.Infrastructure.Tests

# Run a single test by name filter
dotnet test --filter "FullyQualifiedName~RegisterCommandHandlerTests"

# Format code
dotnet format

# Start infrastructure (PostgreSQL, Redis, MongoDB, Elasticsearch, Seq, MailHog)
docker-compose up -d
```

## Architecture

.NET 8 Clean Architecture with CQRS. Four source projects with dependencies flowing inward:

```
API → Application → Domain
API → Infrastructure → Application → Domain
```

- **Domain** (`src/FitTrackPro.Domain/`) — Entities, value objects, enums, domain events, repository interfaces. Zero external dependencies. Entities use factory methods (`User.Create(...)`) that raise domain events.
- **Application** (`src/FitTrackPro.Application/`) — CQRS handlers via MediatR, FluentValidation validators, AutoMapper profiles, pipeline behaviors (validation, logging). Defines interfaces for infrastructure services in `Common/Interfaces/`.
- **Infrastructure** (`src/FitTrackPro.Infrastructure/`) — EF Core (PostgreSQL), MongoDB repositories, Redis caching, Elasticsearch search, JWT auth, email (MailKit), file storage, Hangfire background jobs, ML.NET predictions. All registered in `DependencyInjection.cs`.
- **API** (`src/FitTrackPro.API/`) — Thin controllers that dispatch to MediatR. Setup extensions in `Extensions/`. Global `ExceptionHandlingMiddleware` maps `ValidationException` to 400 and others to 500.

## Key Patterns

**CQRS + MediatR**: Features organized as `Features/{Domain}/{Commands|Queries}/{Name}/`. Each has a request class (`IRequest<Result<T>>`) and handler (`IRequestHandler`). Controllers call `_mediator.Send(command)`.

**Result pattern**: All handlers return `Result<T>` with `IsSuccess`, `Value`, and `Error` properties. Controllers map success to `Ok()` and failure to `BadRequest()`/`Unauthorized()`.

**Domain events**: Entities implement `IHasDomainEvents`. Events are collected during `SaveChangesAsync`, then published via MediatR after the save completes (see `ApplicationDbContext.SaveChangesAsync`).

**Dual database**: PostgreSQL for relational data (users, goals, foods, exercises) via EF Core `ApplicationDbContext`. MongoDB for append-heavy data (meal logs, workouts, personal records) via repository pattern (`IMealLogRepository`, `IWorkoutRepository`, `IPersonalRecordRepository`).

**Auditing**: Entities implementing `IAuditableEntity` get `CreatedAt`/`UpdatedAt` set automatically in `SaveChangesAsync`.

**ML — two separate projects serving different purposes**:
- `src/FitTrackPro.Infrastructure/MachineLearning/` — Runtime weight prediction service (`GoalPredictionService` implements `IGoalPredictionService`). Uses FastTree regression on per-user historical data. Active and wired into the API via `GET /api/analytics/goal-prediction`.
- `src/FitTrackPro.ML/` — Standalone offline training tool for food image classification using ResNet CNN. Produces a trained model `.zip` file. Not yet integrated into the API — will need an `IFoodClassificationService` interface and a controller endpoint to consume the trained model.

## Testing Patterns

- **xUnit** framework, **Moq** for mocking, **FluentAssertions** for assertions
- Test naming: `[Method]_[Scenario]_Should[Expected]`
- **Unit tests** extend `TestBase` which provides pre-configured mocks (`ContextMock`, `JwtTokenGeneratorMock`, `EmailServiceMock`, etc.) and a `CreateDbSetMock<T>()` helper
- **`FakeInMemoryDbContext`** — real EF Core in-memory implementation of `IApplicationDbContext` for tests that need actual DB operations
- **Integration tests** extend `IntegrationTestBase` which uses `CustomWebApplicationFactory` (SQLite in-memory, fake JWT/email/cache services, `TestAuthHandler`). Helpers: `RegisterAndLoginUserAsync()`, `SetAuthorizationHeader()`, `SeedFoodAsync()`
- Integration tests authenticate via `Authorization` header + `X-Test-UserId` header through `TestAuthHandler`

## Infrastructure Services (docker-compose)

PostgreSQL (5432), Redis (6379), MongoDB (27017), Elasticsearch (9200), Seq logging (5341), MailHog email (8025). Admin UIs: pgAdmin (5050), Redis Commander (8081), Mongo Express (8082), Kibana (5601).

## Configuration

`appsettings.json` has connection strings for all databases, JWT settings (`Secret`, `Issuer`, `Audience`), Serilog config, file storage provider setting (`Local`/`Azure`/`Cloudinary`), and email SMTP settings. JWT secret must be at least 64 bytes. The API runs on port 5000 in Docker.
