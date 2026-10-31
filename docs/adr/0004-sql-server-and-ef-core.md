# 0004. SQL Server with EF Core and migrations

- **Status:** Accepted
- **Date:** 2026-10-31

## Context

The data is relational and small: users who own grocery items, with uniqueness rules (username;
item name per user) that must hold under concurrent requests. The company stack is .NET on Azure,
where Azure SQL is the natural target. Reviewers need to run the database locally with no
installation.

## Decision

- **Database:** SQL Server 2022, run locally in Docker (`docker-compose.yml`) and in the
  integration tests through Testcontainers.
- **Access:** EF Core 10 with the SQL Server provider and `EnableRetryOnFailure`. Mapping uses
  `IEntityTypeConfiguration` classes in Infrastructure, so the Domain entities stay free of
  attributes.
- **Constraints in the database:** unique `IX_Users_Username`, unique `IX_GroceryItems_UserId_Name`,
  and a cascading FK from items to users. Uniqueness is case-insensitive through the default
  collation, matching the Application's checks.
- **Race-safe uniqueness:** `AppDbContext` translates SQL errors 2601/2627 into the
  Application-level `UniqueConstraintViolationException`, so a race that passes the pre-check still
  returns a clean 409.
- **Schema changes:** EF Core migrations (`dotnet-ef`, pinned as a local tool). Applying them on
  start-up is opt-in (`Database:ApplyMigrationsOnStartup`), and only the Docker demo enables it.

## Consequences

- **Integrity:** the database enforces the rules even if application code has a bug or two
  requests race.
- **Realistic tests:** integration tests run against the same engine as production. They need
  Docker, and fail fast with a clear message without it.
- **Portable to Azure:** the provider, collation behaviour and migrations carry over to Azure SQL
  unchanged.
- **Heavy local container:** SQL Server needs ~2 GB of memory and a strong `MSSQL_SA_PASSWORD`. The
  password must not contain `;`, because it is embedded in a connection string.
- **Tied to the collation:** case-insensitive uniqueness depends on the server default. A database
  with a case-sensitive collation would need an explicit collation on those columns.
- **Index fragmentation:** v7 GUID keys do not reduce fragmentation on SQL Server, because SQL
  Server orders `uniqueidentifier` by its last bytes. That doesn't matter at this scale; see
  [known issues](../known-issues.md).
