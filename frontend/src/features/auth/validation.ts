import type { LoginRequest, RegisterRequest } from './types'

// Mirrors the backend rules (User constants and AuthService) so most errors are caught before a
// round trip. The backend stays the source of truth: its errors are still shown per field.
export const USERNAME_MIN_LENGTH = 3
export const USERNAME_MAX_LENGTH = 50
export const FULL_NAME_MAX_LENGTH = 100
export const PASSWORD_MIN_LENGTH = 8
export const PASSWORD_MAX_LENGTH = 128

export type AuthField = 'username' | 'fullName' | 'password'
export type FieldErrors<TField extends string> = Partial<Record<TField, string>>

export function validateLogin(values: LoginRequest): FieldErrors<'username' | 'password'> {
  const errors: FieldErrors<'username' | 'password'> = {}
  if (!values.username.trim()) errors.username = 'Username is required.'
  if (!values.password) errors.password = 'Password is required.'
  return errors
}

export function validateRegistration(values: RegisterRequest): FieldErrors<AuthField> {
  const errors: FieldErrors<AuthField> = {}

  const username = values.username.trim()
  if (!username) {
    errors.username = 'Username is required.'
  } else if (username.length < USERNAME_MIN_LENGTH || username.length > USERNAME_MAX_LENGTH) {
    errors.username = `Username must be between ${USERNAME_MIN_LENGTH} and ${USERNAME_MAX_LENGTH} characters.`
  }

  const fullName = values.fullName.trim()
  if (!fullName) {
    errors.fullName = 'Full name is required.'
  } else if (fullName.length > FULL_NAME_MAX_LENGTH) {
    errors.fullName = `Full name must be at most ${FULL_NAME_MAX_LENGTH} characters.`
  }

  if (!values.password) {
    errors.password = 'Password is required.'
  } else if (values.password.length < PASSWORD_MIN_LENGTH || values.password.length > PASSWORD_MAX_LENGTH) {
    errors.password = `Password must be between ${PASSWORD_MIN_LENGTH} and ${PASSWORD_MAX_LENGTH} characters.`
  }

  return errors
}

/** Which form field a backend error code belongs to (e.g. Auth.UsernameTaken → username). */
export function fieldForAuthError(code: string): AuthField | null {
  if (code.startsWith('Auth.Username')) return 'username'
  if (code.startsWith('Auth.FullName')) return 'fullName'
  if (code.startsWith('Auth.Password')) return 'password'
  return null
}

export function hasErrors(errors: object): boolean {
  return Object.keys(errors).length > 0
}
