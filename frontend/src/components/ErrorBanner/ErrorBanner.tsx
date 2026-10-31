import { Button } from '../Button/Button'
import { AlertIcon } from '../Icon/icons'
import styles from './ErrorBanner.module.scss'

export interface ErrorBannerProps {
  message: string
  onRetry?: () => void
  onDismiss?: () => void
}

/** An error message announced immediately to assistive technology (role="alert"). */
export function ErrorBanner({ message, onRetry, onDismiss }: ErrorBannerProps) {
  return (
    <div role="alert" className={styles.banner}>
      <AlertIcon className={styles.icon} />
      <p className={styles.message}>{message}</p>
      {(onRetry || onDismiss) && (
        <div className={styles.actions}>
          {onRetry && (
            <Button variant="secondary" size="small" onClick={onRetry}>
              Try again
            </Button>
          )}
          {onDismiss && (
            <Button variant="ghost" size="small" onClick={onDismiss}>
              Dismiss
            </Button>
          )}
        </div>
      )}
    </div>
  )
}
