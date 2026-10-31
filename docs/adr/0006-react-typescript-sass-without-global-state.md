# 0006. React + TypeScript + Sass, no global state library

- **Status:** Accepted
- **Date:** 2026-10-31

## Context

The company stack is React, TypeScript and Sass. The SPA has two kinds of state: the session (who
is signed in) and one grocery list. The list must feel instant while shopping, so the toggle is
optimistic, and it must stay correct when a request fails. It must also be usable with a keyboard
and a screen reader. Redux, Zustand or React Query would add concepts and dependencies for very
little state.

## Decision

- **Stack:** Vite + React 19 + TypeScript, with Sass CSS Modules for component styles and design
  tokens in `styles/_variables.scss` (no hard-coded colours in components). No Tailwind or UI kit.
  React Router handles routing.
- **Feature folders:** `features/auth`, `features/groceries`, shared `components/`, and an
  `api/httpClient` that knows nothing about the features. The auth feature plugs in the token and a
  401 handler through `configureHttpClient`.
- **No global state library:**
  - The session lives in a React context (`AuthProvider`).
  - The list lives in a view-model hook, `useGroceryList`, which owns the items, the load and error
    state, the pending set and every action. Components only render.
- **One source of truth for the list.** "To buy", "In basket" and "All" are pure selectors over the
  same items (`groceryViews.ts`), never copies. The selected view is kept in the URL (`?view=`), so
  it survives a reload and works with the back button, and unknown values fall back to "to-buy".
- **Optimistic tag / un-tag with rollback.** The item flips immediately, a ref-based guard ignores
  double clicks per item, and on failure the change is reverted and an error banner (`role="alert"`)
  explains it. Adding is not optimistic, because the server assigns the id and may reject
  duplicates.
- **Explicit focus management.** A toggle takes its focused button away: in a filtered view the row
  disappears, and in "All" React moves the row, which blurs it. So `useFocusAfterToggle` moves focus
  explicitly:
  - in "All": to the same item at its new position;
  - in a filtered view: to the next row, else the previous one, else the empty-state heading.

  A polite live region announces "Milk moved to basket". Row buttons use `aria-disabled` while
  pending, so they keep focus.

## Consequences

- **Few dependencies:** the runtime needs only React, React Router and Sass. Behaviour is easy to
  test with Vitest and Testing Library (41 tests), at the hook level and the page level.
- **One place for list logic:** derived views cannot drift out of sync, and filtering costs no
  extra requests.
- **Explicit focus:** focus handling is code that has to be maintained and tested, not left to the
  browser.
- **No caching:** there is no cache or automatic refetch. The list is loaded once per visit, and
  changes made in another tab show up only after a reload. That is acceptable for a personal list,
  and a library like React Query could be added if the list became shared.
- **Hand-written types:** request and response types and validation rules mirror the backend by
  hand ([ADR 0002](0002-monorepo-backend-and-frontend.md)).
