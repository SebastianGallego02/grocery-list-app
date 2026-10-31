import { vi } from 'vitest'
import type { AuthContextValue } from '../features/auth/AuthContext'

/** A controllable AuthContext value, so components can be tested without the real provider. */
export function createAuthValue(overrides: Partial<AuthContextValue> = {}): AuthContextValue {
  return {
    status: 'anonymous',
    user: null,
    login: vi.fn<AuthContextValue['login']>().mockResolvedValue(undefined),
    register: vi.fn<AuthContextValue['register']>().mockResolvedValue(undefined),
    logout: vi.fn<AuthContextValue['logout']>(),
    ...overrides,
  }
}
