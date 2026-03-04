import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { authService, type RegistrationRequest } from '../../infrastructure/services/authService'
import { useToast } from '../components/ui/ToastProvider'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'

export function RegisterPage() {
  const { showToast } = useToast()
  const navigate = useNavigate()

  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [userName, setUserName] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [loading, setLoading] = useState(false)

  async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault()

    if (password !== confirmPassword) {
      showToast('兩次輸入的密碼不一致。', 'error')
      return
    }

    setLoading(true)
    try {
      const payload: RegistrationRequest = {
        firstName,
        lastName,
        email,
        userName,
        password,
      }

      await authService.register(payload)
      showToast('註冊成功，請使用帳號登入。', 'success')
      navigate('/login', { replace: true })
    } catch (error) {
      const err = error as Error
      showToast(err.message ?? '註冊失敗，請稍後再試。', 'error')
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
          <h1 className="mt-2 text-2xl font-semibold">Create an account</h1>
          <p className="mt-1 text-xs text-slate-400">
            註冊新帳號以申請與管理請假。
          </p>
        </div>

        <Card className="bg-slate-900/60 border-slate-700 text-slate-100">
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1">
                <label className="block text-xs font-medium text-gray-900">
                  First name
                </label>
                <input
                  type="text"
                  className="w-full rounded-md border border-slate-700 bg-slate-900/70 px-3 py-2 text-sm text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={firstName}
                  onChange={(e) => setFirstName(e.target.value)}
                  required
                  placeholder="John"
                />
              </div>
              <div className="space-y-1">
                <label className="block text-xs font-medium text-gray-900">
                  Last name
                </label>
                <input
                  type="text"
                  className="w-full rounded-md border border-slate-700 bg-slate-900/70 px-3 py-2 text-sm text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={lastName}
                  onChange={(e) => setLastName(e.target.value)}
                  required
                  placeholder="Doe"
                />
              </div>
            </div>

            <div className="space-y-1">
              <label className="block text-xs font-medium text-gray-900">
                Email
              </label>
              <input
                type="email"
                className="w-full rounded-md border border-slate-700 bg-slate-900/70 px-3 py-2 text-sm text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                placeholder="you@example.com"
              />
            </div>

            <div className="space-y-1">
              <label className="block text-xs font-medium text-gray-900">
                使用者名稱
              </label>
              <input
                type="text"
                className="w-full rounded-md border border-slate-700 bg-slate-900/70 px-3 py-2 text-sm text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
                value={userName}
                onChange={(e) => setUserName(e.target.value)}
                required
                minLength={6}
                placeholder="至少 6 個字元"
              />
            </div>

            <div className="space-y-1">
              <label className="block text-xs font-medium text-gray-900">
                密碼
              </label>
              <input
                type="password"
                className="w-full rounded-md border border-slate-700 bg-slate-900/70 px-3 py-2 text-sm text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                minLength={6}
                placeholder="至少 6 個字元"
              />
            </div>

            <div className="space-y-1">
              <label className="block text-xs font-medium text-gray-900">
                確認密碼
              </label>
              <input
                type="password"
                className="w-full rounded-md border border-slate-700 bg-slate-900/70 px-3 py-2 text-sm text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
                minLength={6}
                placeholder="請再次輸入密碼"
              />
            </div>

            <Button
              type="submit"
              className="w-full justify-center bg-blue-600 hover:bg-blue-500"
              disabled={loading}
            >
              {loading ? '註冊中...' : '建立帳號'}
            </Button>

            <p className="pt-1 text-center text-[0.75rem] text-slate-500">
              已有帳號？{' '}
              <button
                type="button"
                onClick={() => navigate('/login')}
                className="font-medium text-blue-400 hover:text-blue-300"
              >
                前往登入
              </button>
            </p>
          </form>
        </Card>
      </div>
    </div>
  )
}

