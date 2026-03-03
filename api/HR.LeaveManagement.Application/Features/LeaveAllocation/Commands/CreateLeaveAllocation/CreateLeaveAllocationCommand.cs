using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;


namespace HR.LeaveManagement.Application.Features.LeaveAllocation.Commands.CreateLeaveAllocation
{
    public class CreateLeaveAllocationCommand : IRequest<int>
    {
        public int LeaveTypeId { get; set; }
        public int NumberOfDays { get; set; }
        public int Period { get; set; }
        public string EmployeeId { get; set; }
    }
}