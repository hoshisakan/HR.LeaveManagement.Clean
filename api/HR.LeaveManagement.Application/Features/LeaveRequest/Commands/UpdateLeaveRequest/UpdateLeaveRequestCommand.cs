using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using HR.LeaveManagement.Application.Features.LeaveRequest.Shared;


namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.UpdateLeaveRequest
{
    public class UpdateLeaveRequestCommand : BaseLeaveRequest, IRequest<Unit>
    {
        public int Id { get; set; }
        public string RequestComments { get; set; } = string.Empty;
        public bool Cancelled { get; set; }
    }
}