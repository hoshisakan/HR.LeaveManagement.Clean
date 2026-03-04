import { useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { authService } from '../../infrastructure/services/authService'
import { useToast } from '../components/ui/ToastProvider'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'

export function LoginPage() {
  const { showToast } = useToast()
  const navigate = useNavigate()
  const location = useLocation() as { state?: { from?: Location } }

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [loading, setLoading] = useState(false)

  async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault()
    setLoading(true)
    try {
      await authService.login({ email, password })
      showToast('登入成功，歡迎回來。', 'success')
      const redirectTo =
        location.state?.from?.pathname && location.state.from.pathname !== '/login'
          ? location.state.from.pathname
          : '/admin/leave-types'
      navigate(redirectTo, { replace: true })
    } catch (error) {
      const err = error as Error
      showToast(err.message ?? '登入失敗，請確認帳號密碼。', 'error')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-slate-900 via-slate-950 to-slate-900 px-4">
      <div className="w-full max-w-md">
        <div className="mb-6 text-center text-slate-100">
          <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-400">
            HR LEAVE MANAGEMENT
          </p>
          <h1 className="mt-2 text-2xl font-semibold">
            Admin Sign in
          </h1>
          <p className="mt-1 text-xs text-slate-400">
            使用已註冊帳號登入以管理或申請請假。
          </p>
        </div>

        <Card className="bg-slate-900/60 border-slate-700 text-slate-100">
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-1">
              <label className="block text-xs font-medium text-slate-300">
                Email
              </label>
              <input
                type="email"
                className="w-full rounded-md border border-slate-700 bg-slate-900/70 px-3 py-2 text-sm text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                placeholder="admin@example.com"
              />
            </div>

            <div className="space-y-1">
              <label className="block text-xs font-medium text-slate-300">
                密碼
              </label>
              <input
                type="password"
                className="w-full rounded-md border border-slate-700 bg-slate-900/70 px-3 py-2 text-sm text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                placeholder="請輸入密碼"
              />
            </div>

            <Button
              type="submit"
              className="w-full justify-center bg-blue-600 hover:bg-blue-500"
              disabled={loading}
            >
              {loading ? '登入中...' : '登入'}
            </Button>

            <p className="pt-1 text-center text-[0.75rem] text-slate-500">
              還沒有帳號？{' '}
              <button
                type="button"
                onClick={() => navigate('/register')}
                className="font-medium text-blue-400 hover:text-blue-300"
              >
                立即註冊
              </button>
            </p>
          </form>
        </Card>
      </div>
    </div>
  )
}

