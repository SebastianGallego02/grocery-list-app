import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { ApiError, getErrorMessage } from '../../api/httpClient'
import { Button, ErrorBanner, TextField } from '../../components'
import { AuthLayout } from './AuthLayout'
import styles from './AuthLayout.module.scss'
import type { LoginRequest } from './types'
import { useAuth } from './useAuth'
import { fieldForAuthError, hasErrors, validateLogin, type FieldErrors } from './validation'

type LoginField = keyof LoginRequest

export function LoginPage() {
  const { login } = useAuth()
  const [values, setValues] = useState<LoginRequest>({ username: '', password: '' })
  const [fieldErrors, setFieldErrors] = useState<FieldErrors<LoginField>>({})
  const [formError, setFormError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const update = (field: LoginField) => (value: string) => setValues((current) => ({ ...current, [field]: value }))

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (isSubmitting) {
      return
    }

    const errors = validateLogin(values)
    setFieldErrors(errors)
    setFormError(null)
    if (hasErrors(errors)) {
      return
    }

    setIsSubmitting(true)
    try {
      // On success the session changes and AnonymousRoute redirects to the page the user wanted.
      await login({ username: values.username.trim(), password: values.password })
    } catch (error) {
      const field = error instanceof ApiError ? fieldForAuthError(error.code) : null
      if (field === 'username' || field === 'password') {
        setFieldErrors({ [field]: getErrorMessage(error) })
      } else {
        setFormError(getErrorMessage(error))
      }
      setIsSubmitting(false)
    }
  }

  return (
    <AuthLayout
      title="Log in"
      footer={
        <>
          New here? <Link to="/register">Create an account</Link>
        </>
      }
    >
      {formError && <ErrorBanner message={formError} />}
      <form className={styles.form} onSubmit={handleSubmit} noValidate>
        <TextField
          label="Username"
          name="username"
          autoComplete="username"
          value={values.username}
          onChange={(event) => update('username')(event.target.value)}
          error={fieldErrors.username}
          autoFocus
        />
        <TextField
          label="Password"
          name="password"
          type="password"
          autoComplete="current-password"
          value={values.password}
          onChange={(event) => update('password')(event.target.value)}
          error={fieldErrors.password}
        />
        <Button type="submit" isLoading={isSubmitting}>
          {isSubmitting ? 'Logging in…' : 'Log in'}
        </Button>
      </form>
    </AuthLayout>
  )
}
