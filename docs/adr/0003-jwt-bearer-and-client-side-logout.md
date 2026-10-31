# 0003. Stateless JWT bearer tokens and client-side logout

- **Status:** Accepted
- **Date:** 2026-10-31

## Context

The SPA and the API run on different origins (`localhost:3000` and `localhost:8080` in Docker), and
the API should scale horizontally without shared session state. The stories require register,
login, logout and "no session, no data". There is no external identity provider in scope.

## Decision

- **Tokens:** login and register return a JWT (HMAC-SHA256) with `sub` (user id), `unique_name`,
  `name` and `jti`. Issuer, audience, lifetime (60 minutes by default) and the signing key come from
  validated `JwtOptions`, and the key comes from configuration, never from `appsettings.json`.
- **Validation:** the API validates issuer, audience, signature, lifetime and algorithm (`HS256`
  only). A fallback authorization policy requires an authenticated user on every endpoint not marked
  `[AllowAnonymous]`.
- **Sending the token:** the SPA sends `Authorization: Bearer <token>` explicitly, rather than in a
  cookie, so there is no ambient credential and no CSRF surface.
- **Logout is client-side:** the SPA deletes its stored token. There is no logout endpoint and no
  server-side session or deny-list.
- **Storage:** the SPA stores the token and its expiry in `localStorage`, so a reload keeps the user
  signed in. It verifies the token on load with `GET /api/users/me`.

## Consequences

- **Stateless API:** any instance can validate any token, with no session store.
- **Simple model:** logout is instant and cannot fail.
- **No revocation:** a token stays valid until it expires, even after logout, a password change or
  account deletion. The 60-minute lifetime limits the window. Refresh tokens and revocation (for
  example a `jti` deny-list or a per-user token version) are future work
  ([known issues](../known-issues.md)).
- **XSS exposure:** any script on the page can read `localStorage`. The SPA renders no untrusted
  HTML, and nginx sends basic security headers, but a strict Content-Security-Policy would be the
  next step. An httpOnly cookie would avoid this at the cost of CSRF protection and a different API
  design.
- **Expiry handling:** when a token expires the user must log in again. A 401 on a call that carried
  a token clears the session and redirects to `/login`.
