import type { LeaveType } from './LeaveType'

export interface LeaveAllocation {
  id: number
  numberOfDays: number
  leaveTypeId: number
  period: number
  employeeId: string
  leaveType: LeaveType
}

