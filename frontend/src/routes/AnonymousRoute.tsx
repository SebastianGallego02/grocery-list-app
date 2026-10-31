import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { Spinner } from '../components'
import { useAuth } from '../features/auth/useAuth'
import type { RedirectState } from './ProtectedRoute'

/**
 * Login and register are for signed-out users. Once a session exists (including right after a
 * successful login or registration) the user is sent to where they were going, or to the list.
 */
export function AnonymousRoute() {
  const { status } = useAuth()
  const location = useLocation()

  if (status === 'restoring') {
    return <Spinner label="Restoring your session…" />
  }

  if (status === 'authenticated') {
    const from = (location.state as RedirectState | null)?.from
    return <Navigate to={from && from.startsWith('/') ? from : '/'} replace />
  }

  return <Outlet />
}
