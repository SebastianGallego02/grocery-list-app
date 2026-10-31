# Known issues and future work

Limitations of the current implementation, why they are acceptable for this demo, and what would
change for production. Items are grouped by area. "Planned" means not implemented.

## Authentication and security

| Issue | Impact | Future work |
|---|---|---|
| **No refresh tokens** | A session lasts one access token (60 minutes by default); then the user logs in again. | Short-lived access tokens plus rotating refresh tokens (httpOnly cookie, stored server-side). |
| **No token revocation** | Logout only deletes the token in the browser ([ADR 0003](adr/0003-jwt-bearer-and-client-side-logout.md)). A copied token stays valid until it expires, even after logout. | A `jti` deny-list or a per-user token version checked on each request; revoke on logout and password change. |
| **Password rehash not implemented** | `PasswordHasher.Verify` accepts `SuccessRehashNeeded` as success but does not upgrade the stored hash, so hashes made with older Identity parameters stay as they are. | On `SuccessRehashNeeded`, re-hash and save during login. |
| **No brute-force protection** | Login has no rate limit or lockout. Timing and messages do not reveal which usernames exist, but passwords can be guessed at network speed. | ASP.NET Core rate limiting on `/api/auth/*` (per IP and per username), and possibly lockout after repeated failures. |
| **Token in `localStorage`** | Readable by any script that runs on the page (XSS). | A strict Content-Security-Policy in nginx, or move to httpOnly cookies with CSRF protection. |
| **No account management** | No password change, profile edit or account deletion. Deleting a user in the database cascades to their items. | Endpoints for these, each revoking existing tokens. |
| **Unknown routes return 401 to anonymous callers** | The fallback "authenticated user" policy runs before routing can report 404, so `GET /api/nope` without a token returns 401, not 404. Authenticated callers get `404 Http.NotFound`. | Acceptable (it reveals nothing); could be changed by authorizing endpoints explicitly instead of with a fallback policy. |
| **Demo secrets in the repository** | `appsettings.Development.json` contains a development JWT key and the example SA password from `.env.example`. They are for local development only and are never used by the Docker or production configuration. | Use user-secrets only (already initialised) and remove the values from the file. |
| **Docker uses the `sa` login** | The API connects as the SQL Server administrator. | A least-privilege database user for the app; migrations under a separate, privileged identity. |
| **OpenAPI and Scalar exposed in the Docker demo** | `OpenApi__Enabled=true` in `docker-compose.yml`, so reviewers can try the API. | Off by default (`appsettings.json`), and off in production. |
| **`/health` is anonymous** | Anyone can see whether the database is reachable (only `Healthy` / `Unhealthy`, no details). | Restrict it to the internal network or the platform's probe. |

## API

| Issue | Impact | Future work |
|---|---|---|
| **One validation error at a time** | Validation stops at the first failing rule (`Result` carries one `Error`). A request with several invalid fields needs several round trips to fix. The SPA validates all fields client-side first, which hides this in practice. | Let validation errors carry a list (field → code, message), returned like `ValidationProblemDetails.errors`. |
| **Items cannot be edited** | Name and quantity are fixed after creation; the user removes and re-adds. | `PATCH /api/grocery-items/{id}` for name and quantity ([ADR 0007](adr/0007-basket-as-sub-resource.md)). |
| **No pagination** | `GET /api/grocery-items` returns the whole list. Fine for a personal grocery list. | Paging if lists became large or shared. |
| **No API versioning** | Only one version, `v1` in the OpenAPI document. Breaking changes would affect the SPA directly. | URL or header versioning when a second client appears. |

## Data and operations

| Issue | Impact | Future work |
|---|---|---|
| **Migrations on start-up** | The Docker demo applies migrations when the API starts. With several instances they would race, the app needs DDL rights, and a failed migration takes the app down. It is off by default and documented in `MigrationExtensions`. | Run an EF Core migration bundle or idempotent script as a pipeline step before deployment. |
| **v7 GUIDs still fragment on SQL Server** | SQL Server orders `uniqueidentifier` by its last six bytes, which are random in v7, so time-ordered ids do not prevent page splits on the clustered key. Negligible at this size. | Use `NEWSEQUENTIALID()`, a sequential GUID generator in SQL Server's byte order, or a non-clustered PK with a clustered `CreatedAtUtc`. |
| **Case-insensitive uniqueness depends on the collation** | It works because the default collation is `CI`; a case-sensitive database would allow "Milk" and "milk". | Set the collation explicitly on `Username` and `Name`, or store a normalised column. |
| **SA password must not contain `;`** | It is interpolated into the API connection string in `docker-compose.yml`. Documented in `.env.example`. | Build the connection string with quoting, or use a separate app login. |
| **No CI/CD, IaC or dependency automation** | Build, tests and the vulnerability check are run by hand. The Azure deployment is a proposal only ([architecture](architecture.md#azure-proposal-not-deployed)). | GitHub Actions (build, test, `dotnet list package --vulnerable`, image publish), Dependabot, Bicep or Terraform. |
| **No telemetry** | Only console logs. No traces, metrics or Application Insights. | OpenTelemetry with the Azure Monitor exporter. |
| **Pinned transitive package** | `System.Security.Cryptography.Pkcs` is pinned for a security advisory ([ADR 0008](adr/0008-pin-vulnerable-transitive-dependencies.md)). | Remove the pin when `Microsoft.Data.SqlClient` depends on a fixed version. |

## Frontend

| Issue | Impact | Future work |
|---|---|---|
| **Types mirror the backend by hand** | A contract change needs a matching manual change in `types.ts` and the validation rules. | Generate the TypeScript types or client from `/openapi/v1.json` in CI. |
| **No refetch or cross-tab sync** | The list is loaded once per visit. Changes from another tab or device appear after a reload. Removing an item that is already gone is handled (404 is treated as success). | Refetch on window focus, or server push if lists become shared. |
| **Session restore is all-or-nothing** | If `GET /api/users/me` fails on load for any reason, including the API being briefly down, the user is signed out instead of being offered a retry. | Treat network errors differently from 401 and offer "Try again". |
| **No end-to-end tests** | Behaviour is covered by unit and component tests (Vitest, jsdom) and by the API integration tests, but no test drives a real browser against the running stack. | Playwright smoke tests for the nine stories, plus an automated accessibility check (axe). |
| **English only** | UI strings are inline. | Extract strings if localisation becomes a requirement. |
