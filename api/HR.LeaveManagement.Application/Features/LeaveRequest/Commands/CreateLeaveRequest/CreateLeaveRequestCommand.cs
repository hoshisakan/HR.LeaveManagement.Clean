using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using HR.LeaveManagement.Application.Features.LeaveRequest.Shared;


namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.CreateLeaveRequest
{
    public class CreateLeaveRequestCommand : BaseLeaveRequest, IRequest<int>
    {
        public string RequestComments { get; set; } = string.Empty;
    }
}