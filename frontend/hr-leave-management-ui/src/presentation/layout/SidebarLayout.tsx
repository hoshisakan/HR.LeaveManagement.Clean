import type { ReactNode } from 'react'
import { useEffect, useState } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import {
  CalendarRange,
  LayoutDashboard,
  Users,
  Settings2,
  LogOut,
} from 'lucide-react'
import { authService, isAdminUser } from '../../infrastructure/services/authService'
import { useToast } from '../components/ui/ToastProvider'

interface Props {
  children: ReactNode
}

const linkBase =
  'flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium transition-colors'
const linkActive = 'bg-blue-600 text-white shadow-sm'
const linkInactive = 'text-gray-700 hover:bg-gray-100'

export function SidebarLayout({ children }: Props) {
  const navigate = useNavigate()
  const { showToast } = useToast()
  const [isAdmin, setIsAdmin] = useState(false)

  useEffect(() => {
    if (typeof window === 'undefined') return
    setIsAdmin(isAdminUser())
  }, [])

  async function handleLogout() {
    await authService.serverLogout()
    authService.logout()
    showToast('您已登出。', 'info')
    navigate('/login', { replace: true })
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 via-white to-slate-100 flex">
      <aside className="fixed inset-y-0 left-0 z-30 w-64 border-r border-slate-200 bg-white/80 backdrop-blur-sm">
        <div className="flex items-center justify-between px-4 py-4 border-b border-slate-200">
          <div>
            <p className="text-xs font-semibold uppercase tracking-widest text-blue-600">
              HR Dashboard
            </p>
            <h1 className="text-sm font-semibold text-slate-900">
              Leave Management
            </h1>
          </div>
          <button
            type="button"
            onClick={handleLogout}
            className="flex items-center gap-2 rounded-full bg-blue-50 px-2 py-1 text-[11px] font-semibold text-blue-700 hover:bg-blue-100"
          >
            <span className="flex h-6 w-6 items-center justify-center rounded-full bg-blue-600 text-[10px] font-bold text-white">
              HR
            </span>
            <LogOut className="h-3 w-3" />
          </button>
        </div>

        <nav className="px-4 py-4 space-y-6 text-sm">
          {isAdmin && (
            <div>
              <p className="mb-2 px-2 text-[0.65rem] font-semibold uppercase tracking-wide text-slate-500">
                管理員 Admin
              </p>
              <ul className="space-y-1">
                <li>
                  <NavLink
                    to="/dashboard"
                    className={({ isActive }) =>
                      `${linkBase} ${isActive ? linkActive : linkInactive}`
                    }
                  >
                    <LayoutDashboard className="h-4 w-4" />
                    <span>Dashboard</span>
                  </NavLink>
                </li>
                <li>
                  <NavLink
                    to="/admin/leave-types"
                    className={({ isActive }) =>
                      `${linkBase} ${isActive ? linkActive : linkInactive}`
                    }
                  >
                    <CalendarRange className="h-4 w-4" />
                    <span>Leave Types</span>
                  </NavLink>
                </li>
                <li>
                  <NavLink
                    to="/admin/leave-allocations"
                    className={({ isActive }) =>
                      `${linkBase} ${isActive ? linkActive : linkInactive}`
                    }
                  >
                    <CalendarRange className="h-4 w-4" />
                    <span>Leave Allocations</span>
                  </NavLink>
                </li>
                <li>
                  <NavLink
                    to="/admin/leave-requests"
                    className={({ isActive }) =>
                      `${linkBase} ${isActive ? linkActive : linkInactive}`
                    }
                  >
                    <CalendarRange className="h-4 w-4" />
                    <span>Leave Requests</span>
                  </NavLink>
                </li>
                <li>
                  <NavLink
                    to="/admin/users"
                    className={({ isActive }) =>
                      `${linkBase} ${isActive ? linkActive : linkInactive}`
                    }
                  >
                    <Users className="h-4 w-4" />
                    <span>Users</span>
                  </NavLink>
                </li>
              </ul>
            </div>
          )}

          <div>
            <p className="mb-2 px-2 text-[0.65rem] font-semibold uppercase tracking-wide text-slate-500">
              員工 Employee
            </p>
            <ul className="space-y-1">
              <li>
                <NavLink
                  to="/employee/leave-requests"
                  className={({ isActive }) =>
                    `${linkBase} ${isActive ? linkActive : linkInactive}`
                  }
                >
                  <CalendarRange className="h-4 w-4" />
                  <span>My Leave Requests</span>
                </NavLink>
              </li>
            </ul>
          </div>

          <div className="pt-2 border-t border-slate-100">
            <button
              type="button"
              className="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-xs font-medium text-slate-500 hover:bg-slate-100"
            >
              <Settings2 className="h-4 w-4" />
              偏好設定（預留）
            </button>
          </div>
        </nav>
      </aside>

      <main className="flex-1 pl-64">
        <div className="mx-auto max-w-6xl px-6 py-6">{children}</div>
      </main>
    </div>
  )
}
