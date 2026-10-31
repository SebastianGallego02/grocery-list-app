import type { ReactNode } from 'react'
import styles from './EmptyState.module.scss'

export interface EmptyStateProps {
  title: string
  /** Decorative illustration shown above the title. */
  icon?: ReactNode
  children?: ReactNode
}

export function EmptyState({ title, icon, children }: EmptyStateProps) {
  return (
    <div className={styles.empty}>
      {icon && <div className={styles.icon}>{icon}</div>}
      <p className={styles.title}>{title}</p>
      {children && <p className={styles.description}>{children}</p>}
    </div>
  )
}
