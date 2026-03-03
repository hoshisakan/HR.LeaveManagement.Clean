using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;


namespace HR.LeaveManagement.Application.Features.LeaveType.Queries.GetLeaveTypeDetails
{
    public class GetLeaveTypeDetailsQuery : IRequest<LeaveTypeDetailDto>
    {
        public int Id { get; set; }
    }

    // alternative way to define the query
    // public record GetLeaveTypeDetailsQuery(int id) : IRequest<LeaveTypeDetailDto>;
}