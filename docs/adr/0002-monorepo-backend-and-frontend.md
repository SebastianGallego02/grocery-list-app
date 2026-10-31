# 0002. One repository for backend and frontend

- **Status:** Accepted
- **Date:** 2026-10-31

## Context

The application has a .NET API and a React SPA, built and reviewed by the same person. Reviewers
should be able to clone one thing, read the documentation and run everything with one command.
Changes often touch both sides, for example a new error code shown in the UI.

## Decision

Keep both in one Git repository with clear top-level boundaries:

- `backend/`: the .NET solution (`GroceryList.slnx`), its own `Dockerfile` and `.gitignore`.
- `frontend/`: the Vite project, its own `Dockerfile`, nginx config and `.gitignore`.
- `docs/`: architecture, API reference, ADRs and known issues.
- At the root: `docker-compose.yml` and `.env.example`, which run SQL Server, the API and the web
  app together.

The two applications share **no code and no build**. Each side builds and runs on its own. Their
only coupling is the HTTP contract ([`api.md`](../api.md), published as OpenAPI).

## Consequences

- **Atomic changes:** one commit or pull request can change the contract and both of its sides.
- **Simple onboarding:** `docker compose up -d --build` starts the whole system.
- **Independent builds:** each side can be built, tested and deployed separately, because there is
  no shared package or workspace tooling.
- **Shared history:** a pipeline needs path filters (`backend/**`, `frontend/**`) to avoid
  rebuilding both sides on every change.
- **Manual type sync:** the TypeScript types mirror the C# records by hand. Generating a client
  from the OpenAPI document is possible future work ([known issues](../known-issues.md)).
