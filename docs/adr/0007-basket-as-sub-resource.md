# 0007. The basket as a sub-resource (`PUT`/`DELETE …/basket`)

- **Status:** Accepted
- **Date:** 2026-10-31

## Context

Stories 6 and 7 tag a product as picked up and un-tag it. The common options are:

1. `PATCH /api/grocery-items/{id}` with `{ "isInBasket": true }`: a general partial update.
2. `POST /api/grocery-items/{id}/toggle`: an RPC-style action.
3. Treat "in the basket" as a sub-resource that exists or not:
   `PUT /api/grocery-items/{id}/basket` and `DELETE /api/grocery-items/{id}/basket`.

The SPA updates optimistically and may retry on flaky mobile connections, so the operations must be
safe to repeat. A toggle is not: repeating it undoes it.

## Decision

Use option 3. `PUT …/basket` sets "in the basket" and `DELETE …/basket` clears it. Neither takes a
body, both return `204 No Content`, and both return `404 GroceryItems.NotFound` for unknown or
foreign items. In the Domain they map to `GroceryItem.PutInBasket()` and `RemoveFromBasket()`.

## Consequences

- **Idempotent:** per HTTP semantics, repeating a request leaves the same state, so retries and
  double submissions are harmless.
- **Says what it does:** the endpoints name the intent, the target state, rather than a generic
  field update. There is no partial-update body to validate.
- **Room to grow:** other item changes (name, quantity) can come later as a separate `PATCH` or
  `PUT` on the item without overloading this one ([known issues](../known-issues.md)).
- **No body returned:** the response has no content, so the client keeps its own state. That fits
  the optimistic UI: the client already knows the new state.
- **Unfamiliar to some:** some developers expect `PATCH` for this. The API reference documents the
  convention.
