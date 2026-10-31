/**
 * Persists the access token in localStorage so a page reload keeps the user signed in.
 * Only the token and its expiry are stored; the profile is fetched again with GET /api/users/me.
 *
 * Trade-off: localStorage is readable by any script on the page (XSS). An httpOnly cookie would
 * avoid that but needs CSRF protection and a different API design; acceptable for this demo.
 */

const STORAGE_KEY = 'groceryList.session'

export interface StoredSession {
  token: string
  expiresAtUtc: string
}

export function saveSession(session: StoredSession): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify({ token: session.token, expiresAtUtc: session.expiresAtUtc }))
}

/** Returns the stored session, or null (and clears it) when missing, corrupt or expired. */
export function loadSession(now: Date = new Date()): StoredSession | null {
  const raw = localStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }

  try {
    const session = JSON.parse(raw) as Partial<StoredSession>
    if (typeof session.token === 'string' && typeof session.expiresAtUtc === 'string'
        && new Date(session.expiresAtUtc) > now) {
      return { token: session.token, expiresAtUtc: session.expiresAtUtc }
    }
  } catch {
    // Corrupt value: treat as signed out.
  }

  clearSession()
  return null
}

export function clearSession(): void {
  localStorage.removeItem(STORAGE_KEY)
}
