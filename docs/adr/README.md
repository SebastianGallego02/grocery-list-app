# Architecture decision records

Short records of the decisions that shaped this codebase: the context, the decision, and its
consequences. They were written down on 2026-10-31, after the decisions were taken during
development, and describe the code as it is.

| # | Decision | Status |
|---|---|---|
| [0001](0001-pragmatic-clean-architecture.md) | Pragmatic Clean Architecture, without MediatR or CQRS | Accepted |
| [0002](0002-monorepo-backend-and-frontend.md) | One repository for backend and frontend | Accepted |
| [0003](0003-jwt-bearer-and-client-side-logout.md) | Stateless JWT bearer tokens and client-side logout | Accepted |
| [0004](0004-sql-server-and-ef-core.md) | SQL Server with EF Core and migrations | Accepted |
| [0005](0005-result-pattern-over-exceptions.md) | Result pattern for expected errors, exceptions for unexpected ones | Accepted |
| [0006](0006-react-typescript-sass-without-global-state.md) | React + TypeScript + Sass, no global state library | Accepted |
| [0007](0007-basket-as-sub-resource.md) | The basket as a sub-resource (`PUT`/`DELETE …/basket`) | Accepted |
| [0008](0008-pin-vulnerable-transitive-dependencies.md) | Pin vulnerable transitive dependencies to patched versions | Accepted |

New ADRs take the next number and the same three sections. A superseded ADR stays in place, with
its status changed to "Superseded by NNNN".
