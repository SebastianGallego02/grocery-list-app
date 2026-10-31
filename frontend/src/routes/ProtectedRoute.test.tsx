import { render, screen } from '@testing-library/react'
import { RouterProvider, createMemoryRouter, useLocation } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { AuthContext, type AuthContextValue } from '../features/auth/AuthContext'
import { createAuthValue } from '../test/authTestUtils'
import { ProtectedRoute, type RedirectState } from './ProtectedRoute'

function LoginProbe() {
  const from = (useLocation().state as RedirectState | null)?.from
  return <p>Login page, return to {from}</p>
}

function renderAt(path: string, auth: AuthContextValue) {
  const router = createMemoryRouter(
    [
      {
        element: <ProtectedRoute />,
        children: [{ path: '/', element: <p>My private grocery list</p> }],
      },
      { path: '/login', element: <LoginProbe /> },
    ],
    { initialEntries: [path] },
  )

  render(
    <AuthContext.Provider value={auth}>
      <RouterProvider router={router} />
    </AuthContext.Provider>,
  )

  return router
}

describe('ProtectedRoute', () => {
  it('redirects anonymous users to the login page, remembering where they were going', () => {
    const router = renderAt('/?filter=all', createAuthValue({ status: 'anonymous' }))

    expect(router.state.location.pathname).toBe('/login')
    expect(screen.getByText('Login page, return to /?filter=all')).toBeTruthy()
    expect(screen.queryByText('My private grocery list')).toBeNull()
  })

  it('renders the protected page for authenticated users', () => {
    const router = renderAt(
      '/',
      createAuthValue({ status: 'authenticated', user: { username: 'alice', fullName: 'Alice Smith' } }),
    )

    expect(router.state.location.pathname).toBe('/')
    expect(screen.getByText('My private grocery list')).toBeTruthy()
  })

  it('shows a loading state, and neither the page nor a redirect, while the session is being restored', () => {
    const router = renderAt('/', createAuthValue({ status: 'restoring' }))

    expect(router.state.location.pathname).toBe('/')
    expect(screen.getByRole('status').textContent).toContain('Restoring your session')
    expect(screen.queryByText('My private grocery list')).toBeNull()
  })
})
