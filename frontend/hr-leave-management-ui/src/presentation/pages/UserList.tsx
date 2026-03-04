import { useEffect, useState } from 'react'
import { userService } from '../../infrastructure/services/userService'
import type { User } from '../../domain/entities/User'
import { Card } from '../components/ui/Card'
import { DataTable } from '../components/ui/DataTable'
import { Skeleton } from '../components/ui/Skeleton'
import { useToast } from '../components/ui/ToastProvider'

export function UserList() {
  const { showToast } = useToast()
  const [users, setUsers] = useState<User[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    userService
      .getUsers()
      .then(setUsers)
      .catch((err: Error) => {
        showToast(err.message ?? '載入員工列表失敗', 'error')
      })
      .finally(() => setLoading(false))
  }, [showToast])

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">員工列表</h1>
        <p className="mt-1 text-sm text-slate-500">
          檢視系統中的所有員工帳號。
        </p>
      </div>

      <Card title="Employees">
        {loading ? (
          <div className="space-y-2">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </div>
        ) : users.length === 0 ? (
          <p className="text-sm text-slate-500">尚無員工資料。</p>
        ) : (
          <DataTable
            columns={[
              { id: 'id', header: 'ID' },
              { id: 'name', header: '姓名' },
              { id: 'email', header: 'Email' },
            ]}
          >
            {users.map((user, index) => (
              <tr
                key={user.id}
                className={index % 2 === 0 ? 'bg-white' : 'bg-slate-50/60'}
              >
                <td className="px-4 py-3 text-sm text-slate-900 whitespace-nowrap">
                  {user.id}
                </td>
                <td className="px-4 py-3 text-sm text-slate-900">
                  {user.firstName} {user.lastName}
                </td>
                <td className="px-4 py-3 text-sm text-slate-500">
                  {user.email}
                </td>
              </tr>
            ))}
          </DataTable>
        )}
      </Card>
    </div>
  )
}
