import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { Spinner } from '../components'
import { useAuth } from '../features/auth/useAuth'

/** Location state passed to /login so the user returns to the page they asked for. */
export interface RedirectState {
  from?: string
}

/**
 * User story 9: without a session nothing behind this route renders (so no grocery data is
 * requested); the user is sent to the login page instead.
 */
export function ProtectedRoute() {
  const { status } = useAuth()
  const location = useLocation()

  if (status === 'restoring') {
    return <Spinner label="Restoring your session…" />
  }

  if (status === 'anonymous') {
    const state: RedirectState = { from: `${location.pathname}${location.search}` }
    return <Navigate to="/login" replace state={state} />
  }

  return <Outlet />
}
