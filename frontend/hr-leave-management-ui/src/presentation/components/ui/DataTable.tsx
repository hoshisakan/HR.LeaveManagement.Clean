import type { ReactNode } from 'react'

interface Column {
  id: string
  header: ReactNode
  className?: string
}

interface DataTableProps {
  columns: Column[]
  children: ReactNode
}

export function DataTable({ columns, children }: DataTableProps) {
  return (
    <div className="overflow-x-auto rounded-xl border border-gray-200 bg-white/80 shadow-sm">
      <table className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-50/80">
          <tr>
            {columns.map((col) => (
              <th
                key={col.id}
                className={`px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500 ${col.className ?? ''}`}
              >
                {col.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-100 bg-white">
          {children}
        </tbody>
      </table>
    </div>
  )
}

