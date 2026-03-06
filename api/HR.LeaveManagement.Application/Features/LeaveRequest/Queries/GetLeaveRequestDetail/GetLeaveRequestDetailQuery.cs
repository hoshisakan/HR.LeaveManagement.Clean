using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using HR.LeaveManagement.Application.Features.LeaveRequest.Queries.GetLeaveRequestDetail;


namespace HR.LeaveManagement.Application.Features.LeaveRequest.Queries.GetLeaveRequestDetail
{
    public class GetLeaveRequestDetailQuery : IRequest<LeaveRequestDetailsDto>
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public bool IsAdministrator { get; set; }
    }
}