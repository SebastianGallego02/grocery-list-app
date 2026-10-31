import { Navigate, type RouteObject } from 'react-router-dom'
import { LoginPage } from '../features/auth/LoginPage'
import { RegisterPage } from '../features/auth/RegisterPage'
import { AnonymousRoute } from './AnonymousRoute'
import { AppLayout } from './AppLayout'
import { ProtectedRoute } from './ProtectedRoute'

export const appRoutes: RouteObject[] = [
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <AppLayout />,
        children: [{ index: true, element: <h1>My grocery list</h1> }],
      },
    ],
  },
  {
    element: <AnonymousRoute />,
    children: [
      { path: '/login', element: <LoginPage /> },
      { path: '/register', element: <RegisterPage /> },
    ],
  },
  { path: '*', element: <Navigate to="/" replace /> },
]
