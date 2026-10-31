# 0008. Pin vulnerable transitive dependencies to patched versions

- **Status:** Accepted
- **Date:** 2026-10-31

## Context

`dotnet list package --vulnerable --include-transitive` flagged
`System.Security.Cryptography.Pkcs` 9.0.11, which reaches the Infrastructure project indirectly:
`Microsoft.EntityFrameworkCore.SqlServer` 10.0.14 → `Microsoft.Data.SqlClient` 6.1.7 → `Pkcs` 9.0.11.
That version is affected by the advisories
[GHSA-846f-f764-76mx](https://github.com/advisories/GHSA-846f-f764-76mx) and
[GHSA-2fmv-rv2c-59x7](https://github.com/advisories/GHSA-2fmv-rv2c-59x7). EF Core SqlServer 10.0.14
was already the latest patch, so upgrading the direct dependency was not an option.

The OpenAPI stack was also reviewed: `Microsoft.AspNetCore.OpenApi` 10.0.14 brings in
`Microsoft.OpenApi` 2.12.2, its supported 2.x line. It is **not** pinned, because it has no
advisory and pinning it would only risk drifting from what ASP.NET Core supports.

## Decision

Reference the patched `System.Security.Cryptography.Pkcs` 10.0.14 directly in
`GroceryList.Infrastructure.csproj`. NuGet's nearest-wins rule then resolves the patched version
for the whole graph. A comment in the project file names the advisories and the removal condition.
Only packages with a known advisory are pinned. Everything else follows the versions chosen by
the direct dependencies.

## Consequences

- **Clean report:** `dotnet list package --vulnerable --include-transitive` reports no vulnerable
  packages.
- **Pins must be removed:** each pin has to come out once the upstream package
  (`Microsoft.Data.SqlClient`) depends on a fixed version, or it will mask future updates. The
  comment in the `.csproj` records this.
- **Compatibility risk:** a pin can break a transitive consumer if versions diverge too far. Unit and
  integration tests run against the pinned graph.
- **Not automated:** there is no Dependabot or CI audit step yet, so this check is manual
  (planned; see [known issues](../known-issues.md)).
