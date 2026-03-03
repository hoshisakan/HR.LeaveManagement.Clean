using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HR.LeaveManagement.Application.Features.LeaveRequest.Shared
{
    public abstract class BaseLeaveRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int LeaveTypeId { get; set; }
        public string RequestingEmployeeId { get; set; }
    }
}