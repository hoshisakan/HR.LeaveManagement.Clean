import { useEffect, useState } from 'react'
import { Card } from '../components/ui/Card'
import { DataTable } from '../components/ui/DataTable'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { Skeleton } from '../components/ui/Skeleton'
import { useToast } from '../components/ui/ToastProvider'
import { leaveRequestService } from '../../infrastructure/services/leaveRequestService'
import type { LeaveRequestSummary } from '../../domain/entities/LeaveRequest'

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

export function AdminLeaveRequestsPage() {
  const { showToast } = useToast()
  const [requests, setRequests] = useState<LeaveRequestSummary[]>([])
  const [loading, setLoading] = useState(true)
  const [processingId, setProcessingId] = useState<number | null>(null)

  async function load() {
    setLoading(true)
    try {
      const list = await leaveRequestService.getAll()
      setRequests(list)
    } catch (e) {
      const err = e as Error
      showToast(err.message ?? '載入請假申請失敗', 'error')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void load()
  }, [])

  async function handleDecision(id: number, approved: boolean) {
    setProcessingId(id)
    try {
      await leaveRequestService.setApproval(id, approved)
      showToast(approved ? '已核准此申請。' : '已駁回此申請。', 'success')
      await load()
    } catch (e) {
      const err = e as Error
      showToast(err.message ?? '更新審核結果失敗', 'error')
    } finally {
      setProcessingId(null)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-slate-900">
            Leave Requests 審核
          </h1>
          <p className="mt-1 text-sm text-slate-500">
            檢視所有員工的請假申請，並快速進行核准或駁回。
          </p>
        </div>
      </div>

      <Card title="待審核清單">
        {loading ? (
          <div className="space-y-2">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </div>
        ) : requests.length === 0 ? (
          <p className="text-sm text-slate-500">目前沒有請假申請。</p>
        ) : (
          <DataTable
            columns={[
              { id: 'employee', header: '員工 ID' },
              { id: 'type', header: '假別' },
              { id: 'range', header: '期間' },
              { id: 'requested', header: '申請日期' },
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
                  {r.requestingEmployeeId}
                </td>
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
                <td className="px-4 py-3 text-sm text-slate-900 space-x-2">
                  {!r.cancelled && r.approved === null ? (
                    <>
                      <Button
                        size="sm"
                        disabled={processingId === r.id}
                        onClick={() => handleDecision(r.id, true)}
                      >
                        {processingId === r.id ? '處理中...' : '核准'}
                      </Button>
                      <Button
                        size="sm"
                        variant="outline"
                        disabled={processingId === r.id}
                        onClick={() => handleDecision(r.id, false)}
                      >
                        駁回
                      </Button>
                    </>
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

