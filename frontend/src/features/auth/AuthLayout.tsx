import type { ReactNode } from 'react'
import { BasketIcon } from '../../components'
import styles from './AuthLayout.module.scss'

interface AuthLayoutProps {
  title: string
  children: ReactNode
  footer: ReactNode
}

/** Shared frame for the login and register pages. */
export function AuthLayout({ title, children, footer }: AuthLayoutProps) {
  return (
    <main className={styles.page}>
      <p className={styles.brand}>
        <span className={styles.mark} aria-hidden="true">
          <BasketIcon />
        </span>
        Grocery List
      </p>
      <div className={styles.card}>
        <h1 className={styles.title}>{title}</h1>
        {children}
        <p className={styles.footer}>{footer}</p>
      </div>
    </main>
  )
}
