import type { LeaveType } from './LeaveType'

export interface LeaveRequestSummary {
  id: number
  requestingEmployeeId: string
  leaveType: LeaveType
  dateRequested: string
  startDate: string
  endDate: string
  approved: boolean | null
  cancelled: boolean
}

