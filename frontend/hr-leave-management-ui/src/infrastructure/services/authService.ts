import { apiClient } from '../apiClient'

interface AuthRequest {
  email: string
  password: string
}

export interface RegistrationRequest {
  firstName: string
  lastName: string
  email: string
  userName: string
  password: string
}

interface AuthResponse {
  id: string
  userName: string
  email: string
  token: string
  refreshToken: string
}

const BASE = '/api/v1/Auth'

export const authService = {
  async login(input: AuthRequest): Promise<void> {
    const res = await apiClient<AuthResponse>(`${BASE}/Login`, {
      method: 'POST',
      body: JSON.stringify({
        email: input.email,
        password: input.password,
      }),
      requireAuth: false,
    })

    if (res.token) {
      localStorage.setItem('jwt', res.token)
    }
    if (res.refreshToken) {
      localStorage.setItem('refreshToken', res.refreshToken)
    }
    if (res.id) {
      localStorage.setItem('userId', res.id)
    }
  },

  async register(input: RegistrationRequest): Promise<void> {
    await apiClient<{ userId: string }>(`${BASE}/Register`, {
      method: 'POST',
      body: JSON.stringify({
        firstName: input.firstName,
        lastName: input.lastName,
        email: input.email,
        userName: input.userName,
        password: input.password,
      }),
      requireAuth: false,
    })
  },

  logout(): void {
    localStorage.removeItem('jwt')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('userId')
  },

  async refresh(): Promise<boolean> {
    if (typeof window === 'undefined') return false

    const jwt = localStorage.getItem('jwt')
    const refreshToken = localStorage.getItem('refreshToken')
    if (!jwt || !refreshToken) return false

    try {
      const res = await apiClient<AuthResponse>(`${BASE}/Refresh-Token`, {
        method: 'POST',
        body: JSON.stringify({
          jwtToken: jwt,
          refreshToken,
        }),
        requireAuth: false,
      })

      if (res.token) {
        localStorage.setItem('jwt', res.token)
      }
      if (res.refreshToken) {
        localStorage.setItem('refreshToken', res.refreshToken)
      }
      if (res.id) {
        localStorage.setItem('userId', res.id)
      }
      return true
    } catch {
      // refresh 失敗視同登出
      authService.logout()
      return false
    }
  },

  async serverLogout(): Promise<void> {
    if (typeof window === 'undefined') return
    const jwt = localStorage.getItem('jwt')
    const refreshToken = localStorage.getItem('refreshToken')
    if (!jwt || !refreshToken) return

    try {
      await apiClient<void>(`${BASE}/Logout`, {
        method: 'POST',
        body: JSON.stringify({
          jwtToken: jwt,
          refreshToken,
        }),
      })
    } catch {
      // API 登出失敗時，不阻止前端清除本地狀態
    }
  },
}

let refreshTimer: number | undefined

export function startAutoRefresh(intervalMs = 5 * 60 * 1000) {
  if (typeof window === 'undefined') return
  if (refreshTimer) {
    window.clearInterval(refreshTimer)
  }

   // 先嘗試刷新一次，目前 token 若已快過期可立即續期
   void authService.refresh()

  refreshTimer = window.setInterval(() => {
    void authService.refresh()
  }, intervalMs)
}

export function stopAutoRefresh() {
  if (typeof window === 'undefined') return
  if (refreshTimer) {
    window.clearInterval(refreshTimer)
    refreshTimer = undefined
  }
}

function getCurrentRolesFromToken(token: string): string[] {
  try {
    if (typeof window === 'undefined') return []

    const parts = token.split('.')
    if (parts.length < 2) return []

    const payload = parts[1]
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/')
    const decoded = window.atob(normalized)

    const data = JSON.parse(decoded) as Record<string, unknown>

    const roleClaim =
      data.role ??
      data.roles ??
      data['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']

    if (!roleClaim) return []

    if (Array.isArray(roleClaim)) {
      return roleClaim
        .map((x) => String(x).trim())
        .filter((x) => x.length > 0)
    }

    if (typeof roleClaim === 'string') {
      return roleClaim
        .split(',')
        .map((x) => x.trim())
        .filter((x) => x.length > 0)
    }

    return []
  } catch {
    return []
  }
}

export function getCurrentRoles(): string[] {
  if (typeof window === 'undefined') return []
  const token = localStorage.getItem('jwt')
  if (!token) return []
  return getCurrentRolesFromToken(token)
}

export function isAdminUser(): boolean {
  const roles = getCurrentRoles()
  return roles.some(
    (r) => r === 'Administrator' || r === 'Admin' || r === 'admin',
  )
}

