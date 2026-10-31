import { useId, type InputHTMLAttributes, type ReactNode, type Ref } from 'react'
import styles from './TextField.module.scss'

export interface TextFieldProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'id'> {
  label: string
  /** Validation or server error shown under the input and announced to screen readers. */
  error?: string
  hint?: string
  /** Controls rendered inside the field's border, before and after the input (e.g. a stepper). */
  startAdornment?: ReactNode
  endAdornment?: ReactNode
  ref?: Ref<HTMLInputElement>
}

export function TextField({
  label,
  error,
  hint,
  startAdornment,
  endAdornment,
  className,
  ref,
  ...inputProps
}: TextFieldProps) {
  const id = useId()
  const hintId = hint ? `${id}-hint` : undefined
  const errorId = error ? `${id}-error` : undefined
  const describedBy = [hintId, errorId].filter(Boolean).join(' ') || undefined

  return (
    <div className={[styles.field, className].filter(Boolean).join(' ')}>
      <label htmlFor={id} className={styles.label}>
        {label}
      </label>
      <div className={[styles.control, error ? styles.invalid : ''].filter(Boolean).join(' ')}>
        {startAdornment}
        <input
          {...inputProps}
          ref={ref}
          id={id}
          className={styles.input}
          aria-invalid={error ? true : undefined}
          aria-describedby={describedBy}
        />
        {endAdornment}
      </div>
      {hint && (
        <p id={hintId} className={styles.hint}>
          {hint}
        </p>
      )}
      {error && (
        <p id={errorId} className={styles.error}>
          {error}
        </p>
      )}
    </div>
  )
}
