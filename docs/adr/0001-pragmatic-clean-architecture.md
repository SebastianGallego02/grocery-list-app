# 0001. Pragmatic Clean Architecture, without MediatR or CQRS

- **Status:** Accepted
- **Date:** 2026-10-31

## Context

The backend has two aggregates (`User`, `GroceryItem`), nine user stories and one developer, and it
is evaluated on architecture, code quality and testability. The business rules are small but real:
ownership, uniqueness per user, and input limits. They should be testable without HTTP or a
database, and it should be obvious where each concern lives.

Common .NET templates add MediatR (a request/handler per use case), CQRS (separate read and write
models), AutoMapper and a generic `IRepository<T>`. At this size they add indirection and
ceremony without solving a problem we have.

## Decision

Four projects, with dependencies pointing inward:

- **Domain:** entities with factories that enforce invariants. No dependencies.
- **Application:** one service class per feature (`AuthService`, `GroceryItemService`,
  `UserService`), the `Result` type, request/response records, hand-written mapping extensions,
  and the ports (interfaces) it needs: specific repositories, `IUnitOfWork`, `IPasswordHasher`,
  `ITokenService`, `ICurrentUser`.
- **Infrastructure:** EF Core, the repositories, password hashing and JWT. Implements the ports.
- **Api:** controllers, authentication and error mapping. It is the composition root and the only
  project that references Infrastructure.

There is no MediatR, CQRS, AutoMapper or generic repository. Controllers call services directly.
The `DbContext` doubles as the unit of work. Time comes from `TimeProvider`, and the user id always
comes from `ICurrentUser`.

## Consequences

- **Testable:** business rules are unit-tested with fakes (127 unit tests), and the HTTP pipeline is
  covered by integration tests against a real SQL Server.
- **Easy to follow:** the code has few layers of indirection. A request can be read top to bottom:
  controller → service → repository.
- **Fast to change:** adding a use case means adding a service method and a controller action. No
  handler, mapping profile or pipeline behaviour to register.
- **Weaker rules:** without a mediator pipeline, cross-cutting concerns like validation and logging
  are explicit in each service rather than enforced centrally. That is acceptable at this size and
  can be revisited if the number of use cases grows.
- **Not a rich model:** the Domain is deliberately thin (invariants only). Rules that need data,
  such as name uniqueness, live in the Application services.
