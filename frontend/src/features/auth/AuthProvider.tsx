import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { configureHttpClient, isAbortError } from '../../api/httpClient'
import { AuthContext, type AuthContextValue, type AuthStatus } from './AuthContext'
import * as authApi from './authApi'
import { clearSession, loadSession, saveSession } from './session'
import type { AuthResponse, CurrentUser, LoginRequest, RegisterRequest } from './types'

interface AuthState {
  status: AuthStatus
  user: CurrentUser | null
}

const ANONYMOUS: AuthState = { status: 'anonymous', user: null }

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>(() =>
    loadSession() ? { status: 'restoring', user: null } : ANONYMOUS,
  )

  const signOut = useCallback(() => {
    clearSession()
    setState(ANONYMOUS)
  }, [])

  // Registered before the restore request below; protected pages (the only ones that call the API
  // with a token) render only after restoring finishes, so they always find it in place.
  useEffect(() => {
    configureHttpClient({
      getToken: () => loadSession()?.token ?? null,
      // Expired or revoked token: forget it; ProtectedRoute then redirects to the login page.
      onUnauthorized: signOut,
    })
  }, [signOut])

  // Restore a stored session once on load by asking the API who the token belongs to.
  const isRestoring = state.status === 'restoring'
  useEffect(() => {
    if (!isRestoring) {
      return
    }

    const controller = new AbortController()
    authApi
      .getCurrentUser(controller.signal)
      .then((user) => setState({ status: 'authenticated', user: { username: user.username, fullName: user.fullName } }))
      .catch((error: unknown) => {
        if (!isAbortError(error)) {
          // Invalid token or API unreachable: start signed out rather than half signed in.
          signOut()
        }
      })

    return () => controller.abort()
  }, [isRestoring, signOut])

  const startSession = useCallback((response: AuthResponse) => {
    saveSession(response)
    setState({ status: 'authenticated', user: { username: response.username, fullName: response.fullName } })
  }, [])

  const login = useCallback(
    async (request: LoginRequest) => startSession(await authApi.login(request)),
    [startSession],
  )

  const register = useCallback(
    async (request: RegisterRequest) => startSession(await authApi.register(request)),
    [startSession],
  )

  const value = useMemo<AuthContextValue>(
    () => ({ status: state.status, user: state.user, login, register, logout: signOut }),
    [state, login, register, signOut],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
