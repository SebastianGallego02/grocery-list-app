import type { ReactNode, Ref } from 'react'
import styles from './EmptyState.module.scss'

export interface EmptyStateProps {
  title: string
  /** Decorative illustration shown above the title. */
  icon?: ReactNode
  /** The title is a heading that can receive focus programmatically (e.g. when a view empties). */
  titleRef?: Ref<HTMLHeadingElement>
  children?: ReactNode
}

export function EmptyState({ title, icon, titleRef, children }: EmptyStateProps) {
  return (
    <div className={styles.empty}>
      {icon && <div className={styles.icon}>{icon}</div>}
      <h2 ref={titleRef} tabIndex={-1} className={styles.title}>
        {title}
      </h2>
      {children && <p className={styles.description}>{children}</p>}
    </div>
  )
}
