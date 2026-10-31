# 0005. Result pattern for expected errors, exceptions for unexpected ones

- **Status:** Accepted
- **Date:** 2026-10-31

## Context

Most failures in this API are expected: invalid input, a taken username, wrong credentials, an item
that does not exist or belongs to someone else. Throwing exceptions for them hides the possible
outcomes from method signatures, spreads `try/catch` or exception filters around, and makes normal
control flow expensive and hard to follow. Clients also need stable, machine-readable error codes,
not just status codes.

## Decision

- **Expected failures are values.** Application services return `Result` / `Result<T>`, which
  either succeed or carry one `Error(Code, Message, Type)`. All errors live in one catalogue
  (`Errors.Auth.*`, `Errors.Users.*`, `Errors.GroceryItems.*`), and `ErrorType` is Validation,
  NotFound, Conflict or Unauthorized.
- **One place maps errors to HTTP.** `ResultProblemExtensions.Problem` turns an `Error` into
  ProblemDetails (RFC 9457), with the status taken from `ErrorType` and the code in a `code`
  extension.
- **Unexpected failures stay exceptions.** A global `IExceptionHandler` logs them and returns
  `500 Server.UnexpectedError`, with details only in Development. A request cancelled by the client
  is logged at Information level and answered with 499.
- **Framework responses share the shape.** A 401 challenge, an unknown route, a 405 and malformed
  JSON also get ProblemDetails with a code (`Auth.Unauthenticated`, `Http.NotFound`,
  `Http.MethodNotAllowed`, `Request.Invalid`).
- **Domain throws only on invariant violations.** The Application validates first, so a Domain
  exception means a bug.

## Consequences

- **Explicit outcomes:** signatures state that an operation can fail, and the compiler helps:
  `Error` is non-null when `IsFailure`, and reading `Value` on a failure throws.
- **Simple controllers:** each action is one expression,
  `result.IsSuccess ? Ok(...) : this.Problem(result.Error)`.
- **One error contract** for every response, so the frontend branches on `code`, never on messages.
- **Error unit tests** assert on the returned `Error`, without exception plumbing.
- **Some boilerplate:** each service checks and returns errors explicitly, and composing several
  results is manual (no `Bind`/`Map` helpers, deliberately).
- **One error per response:** validation stops at the first failing rule. Reporting several field
  errors at once would need `Error` to carry a list ([known issues](../known-issues.md)).
