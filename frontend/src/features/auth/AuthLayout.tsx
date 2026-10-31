import type { ReactNode } from 'react'
import heroImage from '../../assets/hero-login.webp'
import { BasketIcon } from '../../components'
import styles from './AuthLayout.module.scss'

// Intrinsic size of the image, so the browser reserves its box before it loads.
const HERO_WIDTH = 1300
const HERO_HEIGHT = 722

interface AuthLayoutProps {
  title: string
  children: ReactNode
  footer: ReactNode
}

/**
 * Shared frame for the login and register pages: a hero image with the brand on one side and
 * the form on the other (stacked as a short banner above the form on smaller screens). The
 * page's only h1 is the form title; the image is decorative and hidden from screen readers.
 */
export function AuthLayout({ title, children, footer }: AuthLayoutProps) {
  return (
    <div className={styles.page}>
      <header className={styles.hero}>
        <img
          className={styles.heroImage}
          src={heroImage}
          alt=""
          aria-hidden="true"
          width={HERO_WIDTH}
          height={HERO_HEIGHT}
          decoding="async"
        />
        <div className={styles.overlay}>
          <p className={styles.brand}>
            <span className={styles.mark} aria-hidden="true">
              <BasketIcon />
            </span>
            Grocery List
          </p>
          <p className={styles.tagline}>Your shopping list, always with you</p>
        </div>
      </header>
      <main className={styles.panel}>
        <div className={styles.content}>
          <h1 className={styles.title}>{title}</h1>
          {children}
          <p className={styles.footer}>{footer}</p>
        </div>
      </main>
    </div>
  )
}
