import {
  createContext,
  useCallback,
  useContext,
  useState,
  useEffect,
  type ReactNode,
} from 'react'
import { CheckCircle2, CircleAlert, Info } from 'lucide-react'

type ToastVariant = 'success' | 'error' | 'info'

interface Toast {
  id: number
  message: string
  variant: ToastVariant
}

interface ToastContextValue {
  showToast: (message: string, variant?: ToastVariant) => void
}

const ToastContext = createContext<ToastContextValue | null>(null)

export function useToast() {
  const ctx = useContext(ToastContext)
  if (!ctx) throw new Error('useToast must be used within ToastProvider')
  return ctx
}

interface ToastProviderProps {
  children: ReactNode
}

export function ToastProvider({ children }: ToastProviderProps) {
  const [toasts, setToasts] = useState<Toast[]>([])

  const showToast = useCallback((message: string, variant: ToastVariant = 'info') => {
    setToasts((prev) => [
      ...prev,
      { id: Date.now() + Math.random(), message, variant },
    ])
  }, [])

  useEffect(() => {
    if (toasts.length === 0) return
    const timers = toasts.map((toast) =>
      setTimeout(
        () =>
          setToasts((current) =>
            current.filter((item) => item.id !== toast.id),
          ),
        3500,
      ),
    )
    return () => timers.forEach((t) => clearTimeout(t))
  }, [toasts])

  return (
    <ToastContext.Provider value={{ showToast }}>
      {children}
      <div className="pointer-events-none fixed inset-x-0 top-4 z-50 flex justify-center px-4">
        <div className="flex w-full max-w-md flex-col gap-2">
          {toasts.map((toast) => (
            <ToastItem key={toast.id} toast={toast} />
          ))}
        </div>
      </div>
    </ToastContext.Provider>
  )
}

function ToastItem({ toast }: { toast: Toast }) {
  const variantStyles: Record<ToastVariant, string> = {
    success: 'bg-emerald-50 text-emerald-900 border-emerald-200',
    error: 'bg-red-50 text-red-900 border-red-200',
    info: 'bg-sky-50 text-sky-900 border-sky-200',
  }

  const Icon =
    toast.variant === 'success'
      ? CheckCircle2
      : toast.variant === 'error'
        ? CircleAlert
        : Info

  return (
    <div
      className={`pointer-events-auto flex items-center gap-2 rounded-xl border px-3 py-2 shadow-sm ${variantStyles[toast.variant]}`}
    >
      <Icon className="h-4 w-4 shrink-0" />
      <p className="text-xs font-medium">{toast.message}</p>
    </div>
  )
}

