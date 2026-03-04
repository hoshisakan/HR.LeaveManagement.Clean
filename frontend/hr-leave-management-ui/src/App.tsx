import { useEffect } from 'react'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { SidebarLayout } from './presentation/layout/SidebarLayout'
import { UserList } from './presentation/pages/UserList'
import { AdminLeaveTypesPage } from './presentation/pages/AdminLeaveTypesPage'
import { AdminLeaveAllocationsPage } from './presentation/pages/AdminLeaveAllocationsPage'
import { AdminLeaveRequestsPage } from './presentation/pages/AdminLeaveRequestsPage'
import { EmployeeLeaveRequestsPage } from './presentation/pages/EmployeeLeaveRequestsPage'
import { LoginPage } from './presentation/pages/LoginPage'
import { RegisterPage } from './presentation/pages/RegisterPage'
import { DashboardPage } from './presentation/pages/DashboardPage'
import { ToastProvider } from './presentation/components/ui/ToastProvider'
import { ProtectedRoute } from './presentation/components/ProtectedRoute'
import { AdminRoute } from './presentation/components/AdminRoute'
import { startAutoRefresh, stopAutoRefresh } from './infrastructure/services/authService'
import './App.css'

function App() {
  useEffect(() => {
    startAutoRefresh()
    return () => {
      stopAutoRefresh()
    }
  }, [])

  return (
    <ToastProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          <Route
            path="/*"
            element={
              <SidebarLayout>
                <Routes>
                  <Route
                    path="/"
                    element={
                      <ProtectedRoute>
                        <DashboardPage />
                      </ProtectedRoute>
                    }
                  />
                  <Route
                    path="/dashboard"
                    element={
                      <ProtectedRoute>
                        <DashboardPage />
                      </ProtectedRoute>
                    }
                  />

                  <Route
                    path="/admin/leave-types"
                    element={
                      <AdminRoute>
                        <AdminLeaveTypesPage />
                      </AdminRoute>
                    }
                  />
                  <Route
                    path="/admin/leave-allocations"
                    element={
                      <AdminRoute>
                        <AdminLeaveAllocationsPage />
                      </AdminRoute>
                    }
                  />
                  <Route
                    path="/admin/leave-requests"
                    element={
                      <AdminRoute>
                        <AdminLeaveRequestsPage />
                      </AdminRoute>
                    }
                  />
                  <Route
                    path="/admin/users"
                    element={
                      <AdminRoute>
                        <UserList />
                      </AdminRoute>
                    }
                  />

                  <Route
                    path="/employee/leave-requests"
                    element={
                      <ProtectedRoute>
                        <EmployeeLeaveRequestsPage />
                      </ProtectedRoute>
                    }
                  />

                  <Route path="*" element={<Navigate to="/" replace />} />
                </Routes>
              </SidebarLayout>
            }
          />
        </Routes>
      </BrowserRouter>
    </ToastProvider>
  )
}

export default App

