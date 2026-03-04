import type { ReactElement } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { isAdminUser } from '../../infrastructure/services/authService'

interface AdminRouteProps {
  children: ReactElement
}

export function AdminRoute({ children }: AdminRouteProps) {
  const location = useLocation()
  const token =
    typeof window !== 'undefined' ? localStorage.getItem('jwt') : null

  if (!token) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  if (!isAdminUser()) {
    return <Navigate to="/employee/leave-requests" replace />
  }

  return children
}
