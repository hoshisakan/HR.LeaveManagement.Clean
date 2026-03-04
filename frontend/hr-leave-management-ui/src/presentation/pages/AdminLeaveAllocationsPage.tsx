import { useEffect, useState } from 'react'
import {
  leaveAllocationService,
  type LeaveAllocationUpdateInput,
} from '../../infrastructure/services/leaveAllocationService'
import { leaveTypeService } from '../../infrastructure/services/leaveTypeService'
import type { LeaveAllocation } from '../../domain/entities/LeaveAllocation'
import type { LeaveType } from '../../domain/entities/LeaveType'
import { Button } from '../components/ui/Button'
import { useToast } from '../components/ui/ToastProvider'

export function AdminLeaveAllocationsPage() {
  const { showToast } = useToast()
  const [items, setItems] = useState<LeaveAllocation[]>([])
  const [leaveTypes, setLeaveTypes] = useState<LeaveType[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [employeeId, setEmployeeId] = useState('')
  const [leaveTypeId, setLeaveTypeId] = useState<number>(0)
  const [numberOfDays, setNumberOfDays] = useState<number>(0)
  const [period, setPeriod] = useState<number>(new Date().getFullYear())
  const [saving, setSaving] = useState(false)
  const [editing, setEditing] = useState<LeaveAllocation | null>(null)

  async function load() {
    setLoading(true)
    setError(null)
    try {
      const [allocs, types] = await Promise.all([
        leaveAllocationService.getAll(),
        leaveTypeService.getAll(),
      ])
      setItems(allocs)
      setLeaveTypes(types)
      if (types.length > 0 && leaveTypeId === 0) {
        setLeaveTypeId(types[0].id)
      }
    } catch (e) {
      const err = e as Error
      setError(err.message ?? '載入失敗')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  function resetFormToDefaults() {
    setEmployeeId('')
    if (leaveTypes.length > 0) {
      setLeaveTypeId(leaveTypes[0].id)
    }
    setNumberOfDays(0)
    setPeriod(new Date().getFullYear())
    setEditing(null)
  }

  function startEdit(item: LeaveAllocation) {
    setEditing(item)
    setEmployeeId(item.employeeId)
    setLeaveTypeId(item.leaveTypeId)
    setNumberOfDays(item.numberOfDays)
    setPeriod(item.period)
  }

  async function handleDelete(item: LeaveAllocation) {
    // eslint-disable-next-line no-alert
    if (!confirm(`確認刪除 ${item.employeeId} 的配額？`)) return
    try {
      await leaveAllocationService.remove(item.id)
      showToast('已刪除配額。', 'success')
      await load()
    } catch (e) {
      const err = e as Error
      showToast(err.message ?? '刪除失敗', 'error')
    }
  }

  async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault()
    setSaving(true)
    setError(null)
    try {
      if (editing) {
        const payload: LeaveAllocationUpdateInput = {
          id: editing.id,
          employeeId,
          leaveTypeId,
          numberOfDays,
          period,
        }
        await leaveAllocationService.update(payload)
        showToast('已更新配額。', 'success')
      } else {
        await leaveAllocationService.create({
          employeeId,
          leaveTypeId,
          numberOfDays,
          period,
        })
        showToast('已建立新的配額。', 'success')
      }
      await load()
      resetFormToDefaults()
    } catch (e) {
      const err = e as Error
      setError(err.message ?? '儲存失敗')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Leave Allocations 管理</h1>
      </div>

      {error && <div className="text-sm text-red-600">{error}</div>}

      {loading ? (
        <div className="text-gray-600">載入中...</div>
      ) : (
        <div className="overflow-x-auto rounded-lg border border-gray-200 bg-white">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Employee Id
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Leave Type
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Days
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Period
                </th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  操作
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {items.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-4 py-6 text-center text-gray-500">
                    尚無資料
                  </td>
                </tr>
              ) : (
                items.map((x) => (
                  <tr key={x.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {x.employeeId}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {x.leaveType.name}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {x.numberOfDays}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {x.period}
                    </td>
                    <td className="px-4 py-3 text-right text-sm space-x-2">
                      <Button
                        type="button"
                        size="sm"
                        variant="outline"
                        onClick={() => startEdit(x)}
                      >
                        編輯
                      </Button>
                      <Button
                        type="button"
                        size="sm"
                        variant="danger"
                        onClick={() => handleDelete(x)}
                      >
                        刪除
                      </Button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}

      <form
        onSubmit={handleSubmit}
        className="bg-white rounded-lg border border-gray-200 p-4 space-y-4 max-w-xl"
      >
        <h2 className="text-lg font-semibold">分配員工假期</h2>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">
              Employee Id
            </label>
            <input
              className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              value={employeeId}
              onChange={(e) => setEmployeeId(e.target.value)}
              required
              placeholder="輸入員工 Id"
              title="Employee Id"
            />
          </div>

          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">
              Leave Type
            </label>
            <select
              className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
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
              天數
            </label>
            <input
              type="number"
              min={0}
              className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              value={numberOfDays}
              onChange={(e) => setNumberOfDays(Number(e.target.value) || 0)}
              required
              placeholder="輸入配額天數"
              title="Number of days"
            />
          </div>

          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">
              Period (年度)
            </label>
            <input
              type="number"
              className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              value={period}
              onChange={(e) => setPeriod(Number(e.target.value) || period)}
              required
              title="Period"
            />
          </div>
        </div>

        <div className="flex justify-end gap-2">
          {editing && (
            <Button
              type="button"
              variant="ghost"
              size="sm"
              onClick={resetFormToDefaults}
              disabled={saving}
            >
              取消編輯
            </Button>
          )}
          <Button type="submit" size="sm" disabled={saving}>
            {saving ? '儲存中...' : editing ? '更新' : '儲存'}
          </Button>
        </div>
      </form>
    </div>
  )
}

