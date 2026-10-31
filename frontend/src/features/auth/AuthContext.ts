import { createContext } from 'react'
import type { CurrentUser, LoginRequest, RegisterRequest } from './types'

/**
 * restoring: a stored token exists and is being checked with GET /api/users/me.
 * authenticated: the user is signed in. anonymous: no valid session.
 */
export type AuthStatus = 'restoring' | 'authenticated' | 'anonymous'

export interface AuthContextValue {
  status: AuthStatus
  user: CurrentUser | null
  /** Rejects with an ApiError when the credentials are wrong or the request fails. */
  login: (request: LoginRequest) => Promise<void>
  /** Registers and signs in. Rejects with an ApiError (validation, username taken...). */
  register: (request: RegisterRequest) => Promise<void>
  /** Client-side only: JWTs are stateless, so signing out means forgetting the token. */
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)
