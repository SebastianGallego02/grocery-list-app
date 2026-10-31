import styles from './Spinner.module.scss'

export interface SpinnerProps {
  /** Announced to screen readers; the animation itself is decorative. */
  label?: string
}

export function Spinner({ label = 'Loading…' }: SpinnerProps) {
  return (
    <div role="status" className={styles.spinner}>
      <span className={styles.circle} aria-hidden="true" />
      <span className="visually-hidden">{label}</span>
    </div>
  )
}
