import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { ApiError } from '../../api/httpClient'
import { createAuthValue } from '../../test/authTestUtils'
import { AuthContext, type AuthContextValue } from './AuthContext'
import { LoginPage } from './LoginPage'

function renderLoginPage(login: AuthContextValue['login']) {
  render(
    <AuthContext.Provider value={createAuthValue({ login })}>
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    </AuthContext.Provider>,
  )
}

describe('LoginPage', () => {
  it('shows the error returned by the API when the credentials are wrong', async () => {
    const user = userEvent.setup()
    const login = vi
      .fn<AuthContextValue['login']>()
      .mockRejectedValue(new ApiError(401, 'Auth.InvalidCredentials', 'The username or password is incorrect.'))
    renderLoginPage(login)

    await user.type(screen.getByLabelText('Username'), ' alice ')
    await user.type(screen.getByLabelText('Password'), 'wrong-password')
    await user.click(screen.getByRole('button', { name: 'Log in' }))

    expect((await screen.findByRole('alert')).textContent).toBe('The username or password is incorrect.')
    expect(login).toHaveBeenCalledWith({ username: 'alice', password: 'wrong-password' })
    expect(screen.getByRole('button', { name: 'Log in' }).hasAttribute('disabled')).toBe(false)
  })

  it('validates required fields before calling the API', async () => {
    const user = userEvent.setup()
    const login = vi.fn<AuthContextValue['login']>()
    renderLoginPage(login)

    await user.click(screen.getByRole('button', { name: 'Log in' }))

    expect(login).not.toHaveBeenCalled()
    expect(screen.getByText('Username is required.')).toBeTruthy()
    expect(screen.getByText('Password is required.')).toBeTruthy()
    expect(screen.getByLabelText('Username').getAttribute('aria-invalid')).toBe('true')
  })

  it('prevents double submission while the request is pending', async () => {
    const user = userEvent.setup()
    const login = vi.fn<AuthContextValue['login']>().mockReturnValue(new Promise(() => {}))
    renderLoginPage(login)

    await user.type(screen.getByLabelText('Username'), 'alice')
    await user.type(screen.getByLabelText('Password'), 'Sup3rSecret!')
    await user.click(screen.getByRole('button', { name: 'Log in' }))
    await user.click(screen.getByRole('button', { name: 'Logging in…' }))

    expect(login).toHaveBeenCalledOnce()
    expect(screen.getByRole('button', { name: 'Logging in…' }).hasAttribute('disabled')).toBe(true)
  })
})
