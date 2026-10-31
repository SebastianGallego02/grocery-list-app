import { RouterProvider, createBrowserRouter } from 'react-router-dom'
import { AuthProvider } from './features/auth/AuthProvider'
import { appRoutes } from './routes/routes'

const router = createBrowserRouter(appRoutes)

export default function App() {
  return (
    <AuthProvider>
      <RouterProvider router={router} />
    </AuthProvider>
  )
}
