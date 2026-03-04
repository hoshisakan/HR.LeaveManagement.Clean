import { apiClient } from '../apiClient'
import type { LeaveRequestSummary } from '../../domain/entities/LeaveRequest'
import type { LeaveType } from '../../domain/entities/LeaveType'

const BASE = '/api/v1/LeaveRequests'

type LeaveTypeDto = Record<string, unknown> & {
  id?: number
  Id?: number
  name?: string
  Name?: string
  defaultDays?: number
  DefaultDays?: number
}

type LeaveRequestListDto = Record<string, unknown> & {
  id?: number
  Id?: number
  requestingEmployeeId?: string
  RequestingEmployeeId?: string
  leaveType?: LeaveTypeDto
  LeaveType?: LeaveTypeDto
  dateRequested?: string
  DateRequested?: string
  startDate?: string
  StartDate?: string
  endDate?: string
  EndDate?: string
  approved?: boolean | null
  Approved?: boolean | null
  cancelled?: boolean
  Cancelled?: boolean
}

function toLeaveType(dto?: LeaveTypeDto): LeaveType {
  const safe = dto ?? {}
  return {
    id: safe.id ?? safe.Id ?? 0,
    name: (safe.name ?? safe.Name ?? '') as string,
    defaultDays: safe.defaultDays ?? safe.DefaultDays ?? 0,
  }
}

function toLeaveRequestSummary(dto: LeaveRequestListDto): LeaveRequestSummary {
  return {
    id: dto.id ?? dto.Id ?? 0,
    requestingEmployeeId:
      dto.requestingEmployeeId ?? dto.RequestingEmployeeId ?? '',
    leaveType: toLeaveType(dto.leaveType ?? dto.LeaveType),
    dateRequested: dto.dateRequested ?? dto.DateRequested ?? '',
    startDate: dto.startDate ?? dto.StartDate ?? '',
    endDate: dto.endDate ?? dto.EndDate ?? '',
    approved: dto.approved ?? dto.Approved ?? null,
    cancelled: dto.cancelled ?? dto.Cancelled ?? false,
  }
}

export interface LeaveRequestCreateInput {
  startDate: string
  endDate: string
  leaveTypeId: number
  requestComments: string
  requestingEmployeeId?: string
}

export const leaveRequestService = {
  async getAll(): Promise<LeaveRequestSummary[]> {
    const list = await apiClient<LeaveRequestListDto[]>(BASE)
    return list.map(toLeaveRequestSummary)
  },

  async create(input: LeaveRequestCreateInput): Promise<number> {
    let requestingEmployeeId = input.requestingEmployeeId ?? ''
    if (!requestingEmployeeId && typeof window !== 'undefined') {
      requestingEmployeeId = localStorage.getItem('userId') ?? ''
    }

    return apiClient<number>(BASE, {
      method: 'POST',
      body: JSON.stringify({
        // BaseLeaveRequest + CreateLeaveRequestCommand
        startDate: input.startDate,
        endDate: input.endDate,
        leaveTypeId: input.leaveTypeId,
        requestComments: input.requestComments,
        requestingEmployeeId,
      }),
    })
  },

  async cancel(id: number): Promise<void> {
    await apiClient<void>(`${BASE}/${id}/cancel`, {
      method: 'PUT',
      // CancelLeaveRequestCommand does not require a body,
      // but we send an empty object to satisfy fetch in some environments.
      body: JSON.stringify({}),
    })
  },

  async setApproval(id: number, approved: boolean): Promise<void> {
    await apiClient<void>(`${BASE}/${id}/approve`, {
      method: 'PUT',
      body: JSON.stringify({
        approved,
      }),
    })
  },
}

