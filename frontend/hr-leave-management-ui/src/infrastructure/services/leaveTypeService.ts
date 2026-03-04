import { apiClient } from '../apiClient'
import type { LeaveType } from '../../domain/entities/LeaveType'

const BASE = '/api/v1/LeaveTypes'

type LeaveTypeDto = Record<string, unknown> & {
  id?: number
  Id?: number
  name?: string
  Name?: string
  defaultDays?: number
  DefaultDays?: number
}

function toLeaveType(dto: LeaveTypeDto): LeaveType {
  return {
    id: dto.id ?? dto.Id ?? 0,
    name: (dto.name ?? dto.Name ?? '') as string,
    defaultDays: dto.defaultDays ?? dto.DefaultDays ?? 0,
  }
}

export interface LeaveTypeCreateInput {
  name: string
  defaultDays: number
}

export interface LeaveTypeUpdateInput extends LeaveTypeCreateInput {
  id: number
}

export const leaveTypeService = {
  async getAll(): Promise<LeaveType[]> {
    const list = await apiClient<LeaveTypeDto[]>(BASE)
    return list.map(toLeaveType)
  },

  async get(id: number): Promise<LeaveType> {
    const dto = await apiClient<LeaveTypeDto>(`${BASE}/${id}`)
    return toLeaveType(dto)
  },

  async create(input: LeaveTypeCreateInput): Promise<number> {
    return apiClient<number>(BASE, {
      method: 'POST',
      body: JSON.stringify({
        name: input.name,
        defaultDays: input.defaultDays,
      }),
    })
  },

  async update(input: LeaveTypeUpdateInput): Promise<void> {
    await apiClient<void>(`${BASE}/${input.id}`, {
      method: 'PUT',
      body: JSON.stringify({
        name: input.name,
        defaultDays: input.defaultDays,
      }),
    })
  },

  async remove(id: number): Promise<void> {
    await apiClient<void>(`${BASE}/${id}`, { method: 'DELETE' })
  },
}

