# CLAUDE.md

Working agreement for AI-assisted development in this repository; see README > AI-assisted development.

## Repository layout
- `backend/`: .NET solution `GroceryList.slnx`, with `src/` and `tests/`. Local tools in
  `backend/.config/dotnet-tools.json` (`dotnet-ef`).
- `frontend/`: React + TypeScript + Vite + Sass SPA, feature-based folders under `src/`
  (`api/`, `components/`, `features/auth/`, `features/groceries/`, `routes/`, `styles/`).
- `docs/`: architecture, API reference, ADRs (`docs/adr/`), known issues.
- `docker-compose.yml`, `.env.example`: SQL Server + API + web for local runs.
- `.github/workflows/ci.yml`: build, tests, vulnerability scan, frontend lint/test/build.

## Architecture (pragmatic Clean Architecture)
- `backend/src/GroceryList.Domain`: entities (`User`, `GroceryItem`) and invariants. No dependencies.
- `backend/src/GroceryList.Application`: use-case services, Result pattern, DTOs, ports
  (`Abstractions/`). Depends only on Domain.
- `backend/src/GroceryList.Infrastructure`: EF Core + SQL Server, repositories, JWT, password
  hashing. Implements the Application ports.
- `backend/src/GroceryList.Api`: controllers, auth, `IExceptionHandler` + ProblemDetails, OpenAPI.
  Composition root, the only project that references Infrastructure.

Dependencies point inward: Api → Infrastructure → Application → Domain. Never the reverse.

## Conventions
- .NET 10, C# latest, nullable enabled, file-scoped namespaces, `sealed` classes by default.
- Everything in English: code, comments, commits, UI, docs.
- async/await end to end; every async method accepts and propagates a `CancellationToken`.
- Expected business errors use the Result pattern; exceptions only for unexpected situations.
  Domain throws `ArgumentException` only for invariant violations.
- Time comes from `TimeProvider`, never `DateTime.UtcNow`.
- The current user's id always comes from `ICurrentUser` (the token), never from request input.
  Another user's item returns 404 Not Found, not 403, so its existence is not revealed.
- Keep it simple: no MediatR, AutoMapper, CQRS or generic repository.
- Ask before adding NuGet or npm packages. One-off tools via `npx`/`dotnet tool` are fine but
  are not added to `package.json`.
- No real secrets in `appsettings.json`; `.env` is git-ignored, only `.env.example` is committed.
- Conventional Commits (`feat:`, `fix:`, `test:`, `docs:`, `chore:`, `refactor:`, …), small
  and focused, one per logical step.

## Testing
- Run dotnet commands from `backend/` and npm commands from `frontend/`.
- Before committing: `dotnet build` and `dotnet test` pass, and for frontend changes
  `npm run lint`, `npm test` and `npm run build` pass.
- Unit tests (`GroceryList.UnitTests`, xUnit + NSubstitute + `FakeTimeProvider`) cover Domain,
  Application and Infrastructure logic. Integration tests (`GroceryList.IntegrationTests`)
  run the API against SQL Server via Testcontainers and need Docker.
- New behaviour comes with tests, including the "other user's item" case for item endpoints.

## Working agreement
- Explain the plan briefly before large changes; work in small steps.
- Ask before deciding anything not covered here.
- Do not modify Domain without asking first.
- Keep the docs (`README.md`, `docs/`) in sync with behaviour changes.
