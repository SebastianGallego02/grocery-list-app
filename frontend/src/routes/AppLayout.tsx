import { Outlet } from 'react-router-dom'
import { Button } from '../components'
import { useAuth } from '../features/auth/useAuth'
import styles from './AppLayout.module.scss'

/** Frame for signed-in pages: app name, current user and logout (user story 8). */
export function AppLayout() {
  const { user, logout } = useAuth()

  return (
    <>
      <header className={styles.header}>
        <div className={styles.inner}>
          <p className={styles.brand}>Grocery List</p>
          <div className={styles.account}>
            {user && (
              <span className={styles.user}>
                Signed in as <strong>{user.fullName}</strong>
              </span>
            )}
            <Button variant="secondary" size="small" onClick={logout}>
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
