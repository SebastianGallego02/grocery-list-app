# Grocery List – Development Assessment (2Share)

## Context
Demo grocery list web app for a job assessment. Evaluated on professionalism, architecture,
code quality, testing and documentation — not visual design. Deadline: Monday noon.
Company stack: C#, React, TypeScript, Sass, REST/JSON, Git, Azure (desirable).

## User stories
1. Register with username, full name, password
2. Login with username and password
3. See own grocery list only — never another user's items
4. Add a product
5. Remove a product
6. Tag a product (put in basket)
7. Un-tag a product (put back)
8. Logout
9. No session = no products visible; login is mandatory

## Repository layout
- `backend/`: .NET solution (`GroceryList.slnx`), `src/` and `tests/`.
- `frontend/`: React app.
- `docs/`: project documentation.
- `docker-compose.yml`, `.env.example`: local infrastructure (SQL Server).

## Architecture (Clean Architecture, pragmatic)
- `backend/src/GroceryList.Domain`: entities (User, GroceryItem), invariants. NO dependencies.
- `backend/src/GroceryList.Application`: use-case services, Result pattern, DTOs, ports (interfaces).
  Depends only on Domain.
- `backend/src/GroceryList.Infrastructure`: EF Core + SQL Server, repositories, JWT, password hashing.
  Implements Application interfaces.
- `backend/src/GroceryList.Api`: ASP.NET Core endpoints, auth config, IExceptionHandler + ProblemDetails,
  OpenAPI. Composition root (only place that references Infrastructure).
- `backend/tests/GroceryList.UnitTests`, `backend/tests/GroceryList.IntegrationTests` (xUnit).
- `frontend/`: React + TypeScript + Vite + Sass (feature-based folders).

Dependency rule: dependencies point inward. Infrastructure → Application → Domain. Never the reverse.

## Conventions
- .NET 10, C# latest, nullable enabled, file-scoped namespaces, sealed classes by default.
- Everything in English: code, comments, commits, UI, docs.
- async/await end to end; every async method accepts and propagates a CancellationToken.
- Expected business errors → Result pattern. Exceptions only for truly unexpected situations.
- Domain throws ArgumentException only for invariant violations (should never happen if
  Application validates first).
- Time comes from `TimeProvider`, never `DateTime.UtcNow` directly.
- The current user's id ALWAYS comes from `ICurrentUser` (the token), never from request input.
  Accessing another user's item returns NotFound (not Forbidden) to avoid leaking existence.
- Keep it simple: no MediatR, no AutoMapper, no CQRS, no generic repository.
  Do not add NuGet/npm packages without asking first.
- Conventional Commits (feat:, fix:, test:, docs:, chore:, refactor:), small and focused.

## Environment
- Windows + PowerShell (use `;` to chain commands, no bash globs).
- Run dotnet commands from `backend/` and npm commands from `frontend/`.
- After each change: `dotnet build` and `dotnet test` must pass before committing
  (and `npm run build` for frontend changes).

## Working agreement
- Explain the plan briefly before large changes.
- Ask before deciding anything not covered here.
- Do not modify Domain without asking first.