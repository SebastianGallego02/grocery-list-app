import type { ReactNode } from 'react'
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
      <div className={styles.card}>
        <p className={styles.brand}>Grocery List</p>
        <h1 className={styles.title}>{title}</h1>
        {children}
        <p className={styles.footer}>{footer}</p>
      </div>
    </main>
  )
}
