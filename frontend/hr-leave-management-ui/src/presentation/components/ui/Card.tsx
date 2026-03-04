import type { ReactNode } from 'react'

interface CardProps {
  title?: string
  description?: string
  children: ReactNode
  className?: string
}

export function Card({ title, description, children, className = '' }: CardProps) {
  return (
    <section
      className={`rounded-xl border border-gray-200 bg-white/80 shadow-sm backdrop-blur-sm ${className}`}
    >
      {(title || description) && (
        <header className="border-b border-gray-100 px-5 py-4">
          {title && <h2 className="text-sm font-semibold text-gray-900">{title}</h2>}
          {description && (
            <p className="mt-1 text-xs text-gray-500">{description}</p>
          )}
        </header>
      )}
      <div className="px-5 py-4">{children}</div>
    </section>
  )
}

