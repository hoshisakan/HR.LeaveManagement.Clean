import type { ReactElement } from 'react'
import { Navigate, useLocation } from 'react-router-dom'

interface ProtectedRouteProps {
  children: ReactElement
}

export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const location = useLocation()
  const token =
    typeof window !== 'undefined' ? localStorage.getItem('jwt') : null

  if (!token) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  return children
}

