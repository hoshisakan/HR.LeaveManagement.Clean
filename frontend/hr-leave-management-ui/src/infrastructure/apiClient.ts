/**
 * API client - reads base URL from .env (VITE_API_URL).
 * Use for all HTTP calls to the backend.
 */
const BASE_URL = import.meta.env.VITE_API_URL as string

if (!BASE_URL) {
  console.warn('VITE_API_URL is not set in .env')
}

export type ApiClientOptions = RequestInit & {
  /** When true, do not send Authorization header (e.g. login). Default true. */
  requireAuth?: boolean
}

/**
 * Performs a fetch to the backend API.
 * @param path - Path relative to VITE_API_URL (e.g. '/api/v1/Users')
 * @param options - Fetch options; requireAuth=false skips Bearer token
 */
export async function apiClient<T>(
  path: string,
  options: ApiClientOptions = {}
): Promise<T> {
  const { requireAuth = true, headers: customHeaders, ...fetchOptions } = options

  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(typeof customHeaders === 'object' && customHeaders !== null
      ? customHeaders
      : {}),
  }

  if (requireAuth) {
    const token = localStorage.getItem('jwt')
    if (token) {
      (headers as Record<string, string>)['Authorization'] = `Bearer ${token}`
    }
  }

  const url = `${BASE_URL}${path}`
  const res = await fetch(url, { ...fetchOptions, headers })

  if (!res.ok) {
    if (res.status === 401) {
      // 全域未授權處理：清除 Token 並導向登入頁
      if (typeof window !== 'undefined') {
        localStorage.removeItem('jwt')
        localStorage.removeItem('refreshToken')
        if (window.location.pathname !== '/login') {
          window.location.href = '/login'
        }
      }
    }
    const body = (await res
      .json()
      .catch(() => ({}))) as { title?: string; message?: string }
    throw new Error(body.title ?? body.message ?? `HTTP ${res.status}`)
  }

  if (res.status === 204) {
    return undefined as T
  }

  return res.json() as Promise<T>
}
