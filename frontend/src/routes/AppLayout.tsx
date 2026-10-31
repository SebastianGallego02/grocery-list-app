import { Outlet } from 'react-router-dom'
import { BasketIcon, Button, LogoutIcon } from '../components'
import { useAuth } from '../features/auth/useAuth'
import styles from './AppLayout.module.scss'

/** Frame for signed-in pages: app name, current user and logout (user story 8). */
export function AppLayout() {
  const { user, logout } = useAuth()

  return (
    <>
      <header className={styles.header}>
        <div className={styles.inner}>
          <p className={styles.brand}>
            <span className={styles.mark} aria-hidden="true">
              <BasketIcon />
            </span>
            Grocery List
          </p>
          <div className={styles.account}>
            {user && (
              <span className={styles.user}>
                <span className={styles.signedInAs}>Signed in as </span>
                <strong className={styles.userName}>{user.fullName}</strong>
              </span>
            )}
            <Button variant="inverse" size="small" onClick={logout}>
              <LogoutIcon />
              Log out
            </Button>
          </div>
        </div>
      </header>
      <main className={styles.main}>
        <Outlet />
      </main>
    </>
  )
}
