import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ArrowRight } from 'lucide-react'
import { Card } from '../components/ui/Card'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { Skeleton } from '../components/ui/Skeleton'
import { DataTable } from '../components/ui/DataTable'
import { leaveTypeService } from '../../infrastructure/services/leaveTypeService'
import { leaveAllocationService } from '../../infrastructure/services/leaveAllocationService'
import { leaveRequestService } from '../../infrastructure/services/leaveRequestService'
import { userService } from '../../infrastructure/services/userService'
import type { LeaveRequestSummary } from '../../domain/entities/LeaveRequest'
import { useToast } from '../components/ui/ToastProvider'

function formatDate(date: string) {
  if (!date) return ''
  const d = new Date(date)
  if (Number.isNaN(d.getTime())) return date
  return d.toLocaleDateString()
}

export function DashboardPage() {
  const navigate = useNavigate()
  const { showToast } = useToast()

  const [loading, setLoading] = useState(true)
  const [leaveTypesCount, setLeaveTypesCount] = useState(0)
  const [allocationsCount, setAllocationsCount] = useState(0)
  const [employeeCount, setEmployeeCount] = useState(0)
  const [totalRequests, setTotalRequests] = useState(0)
  const [pendingRequests, setPendingRequests] = useState(0)
  const [approvedRate, setApprovedRate] = useState<string>('0%')
  const [recentRequests, setRecentRequests] = useState<LeaveRequestSummary[]>([])

  useEffect(() => {
    async function load() {
      setLoading(true)
      try {
        const [types, allocations, requests, users] = await Promise.all([
          leaveTypeService.getAll(),
          leaveAllocationService.getAll(),
          leaveRequestService.getAll(),
          userService.getUsers(),
        ])

        setLeaveTypesCount(types.length)
        setAllocationsCount(allocations.length)
        setEmployeeCount(users.length)

        const total = requests.length
        const pending = requests.filter((r) => r.approved === null).length
        const approved = requests.filter((r) => r.approved === true).length

        setTotalRequests(total)
        setPendingRequests(pending)
        setApprovedRate(total > 0 ? `${Math.round((approved / total) * 100)}%` : '0%')

        const sorted = [...requests].sort(
          (a, b) => new Date(b.dateRequested).getTime() - new Date(a.dateRequested).getTime(),
        )
        setRecentRequests(sorted.slice(0, 5))
      } catch (e) {
        const err = e as Error
        showToast(err.message ?? '載入 Dashboard 資料失敗', 'error')
      } finally {
        setLoading(false)
      }
    }

    void load()
  }, [showToast])

  const metricCards = [
    {
      label: '總請假申請',
      value: loading ? '—' : String(totalRequests),
      annotation: '來自 LeaveRequestsController.Get()',
      color: 'text-emerald-600',
    },
    {
      label: '待審核',
      value: loading ? '—' : String(pendingRequests),
      annotation: 'Approved = null',
      color: 'text-amber-600',
    },
    {
      label: '員工數',
      value: loading ? '—' : String(employeeCount),
      annotation: 'UsersController.GetUsers()',
      color: 'text-sky-600',
    },
    {
      label: '核准率',
      value: loading ? '—' : approvedRate,
      annotation: 'Approved / Total',
      color: 'text-violet-600',
    },
  ]

  return (
    <div className="space-y-6">
      {/* Hero */}
      <Card className="border-none bg-gradient-to-r from-emerald-500 via-emerald-500 to-emerald-600 text-emerald-50 shadow-lg">
        <div className="flex flex-col items-start justify-between gap-4 md:flex-row md:items-center">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.3em] text-emerald-100/80">
              HR WORKBENCH
            </p>
            <h1 className="mt-3 text-2xl font-semibold">
              歡迎來到 HR Leave Dashboard
            </h1>
            <p className="mt-2 max-w-xl text-xs text-emerald-50/90 md:text-sm">
              這裡整合了 LeaveTypesController、LeaveAllocationsController、
              LeaveRequestsController 與 UsersController 的資料，
              讓你即時掌握請假與配額狀態。
            </p>
          </div>
          <Button
            variant="outline"
            size="sm"
            className="border-emerald-200 bg-emerald-50/10 text-emerald-50 hover:border-emerald-100 hover:bg-emerald-50/20"
            leftIcon={<ArrowRight className="h-4 w-4" />}
            onClick={() => navigate('/employee/leave-requests')}
          >
            前往待審核請假
          </Button>
        </div>
      </Card>

      {/* Metrics */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {metricCards.map((m) => (
          <Card key={m.label}>
            <p className="text-xs font-medium text-slate-500">{m.label}</p>
            {loading ? (
              <Skeleton className="mt-2 h-6 w-16" />
            ) : (
              <p className="mt-2 text-2xl font-semibold text-slate-900">
                {m.value}
              </p>
            )}
            <p className={`mt-1 text-[11px] text-slate-400 ${m.color}`}>
              {m.annotation}
            </p>
          </Card>
        ))}
      </div>

      {/* Latest requests + quick overview */}
      <div className="grid gap-4 lg:grid-cols-3">
        <Card
          title="最新請假申請"
          description="來自 LeaveRequestsController.Get() 的最近 5 筆資料。"
          className="lg:col-span-2"
        >
          {loading ? (
            <div className="space-y-2">
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
            </div>
          ) : recentRequests.length === 0 ? (
            <p className="text-sm text-slate-500">目前尚無請假申請。</p>
          ) : (
            <DataTable
              columns={[
                { id: 'type', header: '假別' },
                { id: 'range', header: '期間' },
                { id: 'date', header: '申請日期' },
                { id: 'status', header: '狀態' },
              ]}
            >
              {recentRequests.map((r, index) => (
                <tr
                  key={`${r.requestingEmployeeId}-${index}`}
                  className={index % 2 === 0 ? 'bg-white' : 'bg-slate-50/60'}
                >
                  <td className="px-4 py-3 text-sm text-slate-900">
                    {r.leaveType.name}
                  </td>
                  <td className="px-4 py-3 text-sm text-slate-900">
                    {formatDate(r.startDate)} ~ {formatDate(r.endDate)}
                  </td>
                  <td className="px-4 py-3 text-sm text-slate-900">
                    {formatDate(r.dateRequested)}
                  </td>
                  <td className="px-4 py-3 text-sm text-slate-900">
                    {r.approved === true && (
                      <Badge variant="success">Approved</Badge>
                    )}
                    {r.approved === false && (
                      <Badge variant="danger">Rejected</Badge>
                    )}
                    {r.approved === null && (
                      <Badge variant="warning">Pending</Badge>
                    )}
                  </td>
                </tr>
              ))}
            </DataTable>
          )}
        </Card>

        <Card title="今日重點" description="快速檢視需要關注的項目。">
          {loading ? (
            <Skeleton className="h-20 w-full" />
          ) : (
            <ul className="space-y-2 text-xs text-slate-700">
              <li className="flex items-center justify-between">
                <span>待審核請假</span>
                <Badge variant="warning">{pendingRequests}</Badge>
              </li>
              <li className="flex items-center justify-between">
                <span>請假類型數量</span>
                <Badge variant="outline">{leaveTypesCount}</Badge>
              </li>
              <li className="flex items-center justify-between">
                <span>已建立配額</span>
                <Badge variant="outline">{allocationsCount}</Badge>
              </li>
              <li className="flex items-center justify-between">
                <span>員工人數</span>
                <Badge variant="success">{employeeCount}</Badge>
              </li>
            </ul>
          )}
        </Card>
      </div>
    </div>
  )
}
