import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { ApiError, getErrorMessage } from '../../api/httpClient'
import { Button, ErrorBanner, TextField } from '../../components'
import { AuthLayout } from './AuthLayout'
import styles from './AuthLayout.module.scss'
import type { RegisterRequest } from './types'
import { useAuth } from './useAuth'
import {
  FULL_NAME_MAX_LENGTH,
  PASSWORD_MAX_LENGTH,
  PASSWORD_MIN_LENGTH,
  USERNAME_MAX_LENGTH,
  USERNAME_MIN_LENGTH,
  fieldForAuthError,
  hasErrors,
  validateRegistration,
  type AuthField,
  type FieldErrors,
} from './validation'

export function RegisterPage() {
  const { register } = useAuth()
  const [values, setValues] = useState<RegisterRequest>({ username: '', fullName: '', password: '' })
  const [fieldErrors, setFieldErrors] = useState<FieldErrors<AuthField>>({})
  const [formError, setFormError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const update = (field: AuthField) => (value: string) => setValues((current) => ({ ...current, [field]: value }))

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (isSubmitting) {
      return
    }

    const errors = validateRegistration(values)
    setFieldErrors(errors)
    setFormError(null)
    if (hasErrors(errors)) {
      return
    }

    setIsSubmitting(true)
    try {
      // Registering also signs the user in; AnonymousRoute then redirects to the list.
      await register({
        username: values.username.trim(),
        fullName: values.fullName.trim(),
        password: values.password,
      })
    } catch (error) {
      const field = error instanceof ApiError ? fieldForAuthError(error.code) : null
      if (field) {
        setFieldErrors({ [field]: getErrorMessage(error) })
      } else {
        setFormError(getErrorMessage(error))
      }
      setIsSubmitting(false)
    }
  }

  return (
    <AuthLayout
      title="Create an account"
      footer={
        <>
          Already have an account? <Link to="/login">Log in</Link>
        </>
      }
    >
      {formError && <ErrorBanner message={formError} />}
      <form className={styles.form} onSubmit={handleSubmit} noValidate>
        <TextField
          label="Username"
          name="username"
          autoComplete="username"
          hint={`${USERNAME_MIN_LENGTH}–${USERNAME_MAX_LENGTH} characters.`}
          maxLength={USERNAME_MAX_LENGTH}
          value={values.username}
          onChange={(event) => update('username')(event.target.value)}
          error={fieldErrors.username}
          autoFocus
        />
        <TextField
          label="Full name"
          name="fullName"
          autoComplete="name"
          maxLength={FULL_NAME_MAX_LENGTH}
          value={values.fullName}
          onChange={(event) => update('fullName')(event.target.value)}
          error={fieldErrors.fullName}
        />
        <TextField
          label="Password"
          name="password"
          type="password"
          autoComplete="new-password"
          hint={`${PASSWORD_MIN_LENGTH}–${PASSWORD_MAX_LENGTH} characters.`}
          maxLength={PASSWORD_MAX_LENGTH}
          value={values.password}
          onChange={(event) => update('password')(event.target.value)}
          error={fieldErrors.password}
        />
        <Button type="submit" isLoading={isSubmitting}>
          {isSubmitting ? 'Creating account…' : 'Create account'}
        </Button>
      </form>
    </AuthLayout>
  )
}
