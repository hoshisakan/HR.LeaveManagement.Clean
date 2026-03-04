import { useEffect, useMemo, useState } from 'react'
import { leaveTypeService } from '../../infrastructure/services/leaveTypeService'
import {
  leaveRequestService,
  type LeaveRequestCreateInput,
} from '../../infrastructure/services/leaveRequestService'
import type { LeaveType } from '../../domain/entities/LeaveType'
import type { LeaveRequestSummary } from '../../domain/entities/LeaveRequest'
import { Card } from '../components/ui/Card'
import { Button } from '../components/ui/Button'
import { DataTable } from '../components/ui/DataTable'
import { Badge } from '../components/ui/Badge'
import { Skeleton } from '../components/ui/Skeleton'
import { useToast } from '../components/ui/ToastProvider'

function formatDate(date: string) {
  if (!date) return ''
  const d = new Date(date)
  if (Number.isNaN(d.getTime())) return date
  return d.toLocaleDateString()
}

function statusBadge(approved: boolean | null, cancelled: boolean) {
  if (cancelled) return <Badge variant="default">已取消</Badge>
  if (approved === true) return <Badge variant="success">Approved</Badge>
  if (approved === false) return <Badge variant="danger">Rejected</Badge>
  return <Badge variant="warning">Pending</Badge>
}

export function EmployeeLeaveRequestsPage() {
  const { showToast } = useToast()
  const [leaveTypes, setLeaveTypes] = useState<LeaveType[]>([])
  const [requests, setRequests] = useState<LeaveRequestSummary[]>([])
  const [loading, setLoading] = useState(true)

  const [leaveTypeId, setLeaveTypeId] = useState<number>(0)
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  const [requestComments, setRequestComments] = useState('')
  const [saving, setSaving] = useState(false)
  const [cancellingId, setCancellingId] = useState<number | null>(null)

  async function load() {
    setLoading(true)
    try {
      const [types, list] = await Promise.all([
        leaveTypeService.getAll(),
        leaveRequestService.getAll(),
      ])

      setLeaveTypes(types)
      setRequests(list)
      if (types.length > 0 && leaveTypeId === 0) {
        setLeaveTypeId(types[0].id)
      }
    } catch (e) {
      const err = e as Error
      showToast(err.message ?? '載入請假資料失敗', 'error')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault()
    setSaving(true)
    try {
      const payload: LeaveRequestCreateInput = {
        startDate,
        endDate,
        leaveTypeId,
        requestComments,
      }
      await leaveRequestService.create(payload)
      showToast('已送出請假申請。', 'success')
      await load()
      setStartDate('')
      setEndDate('')
      setRequestComments('')
    } catch (e) {
      const err = e as Error
      showToast(err.message ?? '送出申請失敗', 'error')
    } finally {
      setSaving(false)
    }
  }

  const pendingCount = useMemo(
    () =>
      requests.filter((r) => r.approved === null && !r.cancelled).length,
    [requests],
  )

  async function handleCancel(id: number) {
    setCancellingId(id)
    try {
      await leaveRequestService.cancel(id)
      showToast('已取消此筆請假申請。', 'success')
      await load()
    } catch (e) {
      const err = e as Error
      showToast(err.message ?? '取消申請失敗', 'error')
    } finally {
      setCancellingId(null)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-slate-900">請假申請</h1>
          <p className="mt-1 text-sm text-slate-500">
            建立新的請假申請，並追蹤目前的審核狀態。
          </p>
        </div>
        <div className="text-right text-xs text-slate-500">
          <p>待審核申請：{pendingCount} 筆</p>
        </div>
      </div>

      <Card title="新增請假申請">
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            <div className="space-y-1">
              <label className="block text-sm font-medium text-gray-700">
                假別
              </label>
              <select
                className="w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                value={leaveTypeId}
                onChange={(e) => setLeaveTypeId(Number(e.target.value))}
                required
                title="Leave Type"
              >
                {leaveTypes.length === 0 ? (
                  <option value="">尚無 Leave Types</option>
                ) : (
                  leaveTypes.map((lt) => (
                    <option key={lt.id} value={lt.id}>
                      {lt.name} ({lt.defaultDays}天)
                    </option>
                  ))
                )}
              </select>
            </div>

            <div className="space-y-1">
              <label className="block text-sm font-medium text-gray-700">
                開始日期
              </label>
              <input
                type="date"
                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                required
                title="Start date"
              />
            </div>

            <div className="space-y-1">
              <label className="block text-sm font-medium text-gray-700">
                結束日期
              </label>
              <input
                type="date"
                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                required
                title="End date"
              />
            </div>

            <div className="space-y-1 md:col-span-2">
              <label className="block text-sm font-medium text-gray-700">
                備註
              </label>
              <textarea
                className="min-h-[80px] w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                value={requestComments}
                onChange={(e) => setRequestComments(e.target.value)}
                placeholder="可選填，說明請假原因"
                title="Request comments"
              />
            </div>
          </div>

          <Button type="submit" disabled={saving}>
            {saving ? '送出中...' : '送出申請'}
          </Button>
        </form>
      </Card>

      <Card title="我的申請紀錄">
        {loading ? (
          <div className="space-y-2">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </div>
        ) : requests.length === 0 ? (
          <p className="text-sm text-slate-500">尚無申請紀錄。</p>
        ) : (
          <DataTable
            columns={[
              { id: 'type', header: '假別' },
              { id: 'range', header: '期間' },
              { id: 'date', header: '申請日期' },
              { id: 'status', header: '狀態' },
              { id: 'actions', header: '操作' },
            ]}
          >
            {requests.map((r, index) => (
              <tr
                key={r.id ?? index}
                className={
                  r.cancelled
                    ? 'bg-slate-100 text-slate-500'
                    : index % 2 === 0
                      ? 'bg-white'
                      : 'bg-slate-50/60'
                }
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
                  {statusBadge(r.approved, r.cancelled)}
                </td>
                <td className="px-4 py-3 text-sm text-slate-900">
                  {!r.cancelled && r.approved === null ? (
                    <Button
                      variant="outline"
                      size="sm"
                      disabled={cancellingId === r.id}
                      onClick={() => handleCancel(r.id)}
                    >
                      {cancellingId === r.id ? '取消中...' : '取消申請'}
                    </Button>
                  ) : (
                    <span className="text-xs text-slate-400">—</span>
                  )}
                </td>
              </tr>
            ))}
          </DataTable>
        )}
      </Card>
    </div>
  )
}

