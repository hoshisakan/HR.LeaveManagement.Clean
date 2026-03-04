import { apiClient } from '../apiClient'
import type { LeaveAllocation } from '../../domain/entities/LeaveAllocation'
import type { LeaveType } from '../../domain/entities/LeaveType'

const BASE = '/api/v1/LeaveAllocations'

type LeaveTypeDto = Record<string, unknown> & {
  id?: number
  Id?: number
  name?: string
  Name?: string
  defaultDays?: number
  DefaultDays?: number
}

type LeaveAllocationDto = Record<string, unknown> & {
  id?: number
  Id?: number
  numberOfDays?: number
  NumberOfDays?: number
  leaveTypeId?: number
  LeaveTypeId?: number
  period?: number
  Period?: number
  employeeId?: string
  EmployeeId?: string
  leaveType?: LeaveTypeDto
  LeaveType?: LeaveTypeDto
}

function toLeaveType(dto?: LeaveTypeDto): LeaveType {
  const safe = dto ?? {}
  return {
    id: safe.id ?? safe.Id ?? 0,
    name: (safe.name ?? safe.Name ?? '') as string,
    defaultDays: safe.defaultDays ?? safe.DefaultDays ?? 0,
  }
}

function toLeaveAllocation(dto: LeaveAllocationDto): LeaveAllocation {
  return {
    id: dto.id ?? dto.Id ?? 0,
    numberOfDays: dto.numberOfDays ?? dto.NumberOfDays ?? 0,
    leaveTypeId: dto.leaveTypeId ?? dto.LeaveTypeId ?? 0,
    period: dto.period ?? dto.Period ?? 0,
    employeeId: (dto.employeeId ?? dto.EmployeeId ?? '') as string,
    leaveType: toLeaveType(dto.leaveType ?? dto.LeaveType),
  }
}

export interface LeaveAllocationCreateInput {
  leaveTypeId: number
  numberOfDays: number
  period: number
  employeeId: string
}

export interface LeaveAllocationUpdateInput {
  id: number
  leaveTypeId: number
  numberOfDays: number
  period: number
  employeeId: string
}

export const leaveAllocationService = {
  async getAll(): Promise<LeaveAllocation[]> {
    const list = await apiClient<LeaveAllocationDto[]>(BASE)
    return list.map(toLeaveAllocation)
  },

  async create(input: LeaveAllocationCreateInput): Promise<number> {
    return apiClient<number>(BASE, {
      method: 'POST',
      body: JSON.stringify({
        leaveTypeId: input.leaveTypeId,
        numberOfDays: input.numberOfDays,
        period: input.period,
        employeeId: input.employeeId,
      }),
    })
  },

  async update(input: LeaveAllocationUpdateInput): Promise<void> {
    await apiClient<void>(`${BASE}/${input.id}`, {
      method: 'PUT',
      body: JSON.stringify({
        leaveTypeId: input.leaveTypeId,
        numberOfDays: input.numberOfDays,
        period: input.period,
        employeeId: input.employeeId,
      }),
    })
  },

  async remove(id: number): Promise<void> {
    await apiClient<void>(`${BASE}/${id}`, {
      method: 'DELETE',
    })
  },
}

