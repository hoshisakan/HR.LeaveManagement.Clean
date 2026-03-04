import { useEffect, useMemo, useState } from 'react'
import {
  leaveTypeService,
  type LeaveTypeCreateInput,
  type LeaveTypeUpdateInput,
} from '../../infrastructure/services/leaveTypeService'
import type { LeaveType } from '../../domain/entities/LeaveType'
import { Card } from '../components/ui/Card'
import { Button } from '../components/ui/Button'
import { DataTable } from '../components/ui/DataTable'
import { Skeleton } from '../components/ui/Skeleton'
import { Modal } from '../components/ui/Modal'
import { Badge } from '../components/ui/Badge'
import { useToast } from '../components/ui/ToastProvider'
import { Plus, Pencil, Trash2 } from 'lucide-react'

export function AdminLeaveTypesPage() {
  const { showToast } = useToast()
  const [items, setItems] = useState<LeaveType[]>([])
  const [loading, setLoading] = useState(true)

  const [editing, setEditing] = useState<LeaveType | null>(null)
  const [name, setName] = useState('')
  const [defaultDays, setDefaultDays] = useState<number>(0)
  const [saving, setSaving] = useState(false)
  const [modalOpen, setModalOpen] = useState(false)

  async function load() {
    setLoading(true)
    try {
      const data = await leaveTypeService.getAll()
      setItems(data)
    } catch (e) {
      const err = e as Error
      showToast(err.message ?? '載入 leave types 失敗', 'error')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void load()
  }, [])

  function openCreateModal() {
    setEditing(null)
    setName('')
    setDefaultDays(0)
    setModalOpen(true)
  }

  function openEditModal(item: LeaveType) {
    setEditing(item)
    setName(item.name)
    setDefaultDays(item.defaultDays)
    setModalOpen(true)
  }

  function closeModal() {
    if (saving) return
    setModalOpen(false)
  }

  async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault()
    setSaving(true)

    try {
      if (editing) {
        const payload: LeaveTypeUpdateInput = {
          id: editing.id,
          name,
          defaultDays,
        }
        await leaveTypeService.update(payload)
        showToast('已更新 Leave Type', 'success')
      } else {
        const payload: LeaveTypeCreateInput = {
          name,
          defaultDays,
        }
        await leaveTypeService.create(payload)
        showToast('已建立新的 Leave Type', 'success')
      }
      await load()
      setModalOpen(false)
    } catch (e) {
      const err = e as Error
      showToast(err.message ?? '儲存失敗', 'error')
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete(id: number) {
    // eslint-disable-next-line no-alert
    if (!confirm('確認刪除這個 Leave Type？')) return
    try {
      await leaveTypeService.remove(id)
      showToast('已刪除 Leave Type', 'success')
      await load()
    } catch (e) {
      const err = e as Error
      showToast(err.message ?? '刪除失敗', 'error')
    }
  }

  const totalDefaultDays = useMemo(
    () => items.reduce((sum, lt) => sum + lt.defaultDays, 0),
    [items],
  )

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-slate-900">
            Leave Types
          </h1>
          <p className="mt-1 text-sm text-slate-500">
            管理系統中的請假類型與預設天數。
          </p>
        </div>
        <Button size="sm" leftIcon={<Plus className="h-4 w-4" />} onClick={openCreateModal}>
          新增 Leave Type
        </Button>
      </div>

      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        <Card title="類型數量" className="col-span-1">
          {loading ? (
            <Skeleton className="h-6 w-20" />
          ) : (
            <p className="text-2xl font-semibold text-slate-900">
              {items.length}
            </p>
          )}
        </Card>
        <Card title="總預設天數" className="col-span-1">
          {loading ? (
            <Skeleton className="h-6 w-24" />
          ) : (
            <p className="text-2xl font-semibold text-slate-900">
              {totalDefaultDays}
              <span className="ml-1 text-sm text-slate-500">天</span>
            </p>
          )}
        </Card>
        <Card title="狀態" className="col-span-1">
          <Badge variant="success">系統正常運作中</Badge>
        </Card>
      </div>

      <Card title="Leave Types 清單" description="所有類型一覽與操作。">
        {loading ? (
          <div className="space-y-2">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </div>
        ) : items.length === 0 ? (
          <p className="text-sm text-slate-500">目前尚無 Leave Types。</p>
        ) : (
          <DataTable
            columns={[
              { id: 'id', header: 'ID' },
              { id: 'name', header: '名稱' },
              { id: 'days', header: 'Default Days' },
              { id: 'actions', header: '操作', className: 'text-right' },
            ]}
          >
            {items.map((x, index) => (
              <tr
                key={x.id}
                className={index % 2 === 0 ? 'bg-white' : 'bg-slate-50/60'}
              >
                <td className="px-4 py-3 text-sm text-slate-900">{x.id}</td>
                <td className="px-4 py-3 text-sm text-slate-900">{x.name}</td>
                <td className="px-4 py-3 text-sm text-slate-900">
                  {x.defaultDays}
                </td>
                <td className="px-4 py-3 text-right text-sm">
                  <Button
                    type="button"
                    size="sm"
                    variant="outline"
                    className="mr-2"
                    leftIcon={<Pencil className="h-3 w-3" />}
                    onClick={() => openEditModal(x)}
                  >
                    編輯
                  </Button>
                  <Button
                    type="button"
                    size="sm"
                    variant="danger"
                    leftIcon={<Trash2 className="h-3 w-3" />}
                    onClick={() => handleDelete(x.id)}
                  >
                    刪除
                  </Button>
                </td>
              </tr>
            ))}
          </DataTable>
        )}
      </Card>

      <Modal
        open={modalOpen}
        onClose={closeModal}
        title={editing ? '編輯 Leave Type' : '新增 Leave Type'}
        description="設定名稱與預設天數。"
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">
              名稱
            </label>
            <input
              className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              placeholder="請輸入名稱"
              title="名稱"
            />
          </div>

          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">
              Default Days
            </label>
            <input
              type="number"
              min={0}
              className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              value={defaultDays}
              onChange={(e) => setDefaultDays(Number(e.target.value) || 0)}
              required
              placeholder="請輸入預設天數"
              title="Default Days"
            />
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <Button
              type="button"
              variant="ghost"
              size="sm"
              onClick={closeModal}
              disabled={saving}
            >
              取消
            </Button>
            <Button type="submit" size="sm" disabled={saving}>
              {saving ? '儲存中...' : '儲存'}
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}
