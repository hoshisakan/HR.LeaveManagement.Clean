using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using HR.LeaveManagement.Application.Features.LeaveAllocation.Queries.GetLeaveAllocations;


namespace HR.LeaveManagement.Application.Features.LeaveAllocation.Queries.GetLeaveAllocations
{
    public class GetLeaveAllocationListQuery : IRequest<List<LeaveAllocationDto>>
    {
        public string? UserId { get; set; }
        public bool IsAdministrator { get; set; }
    }

    // alternative way to define the query
    // public record GetLeaveAllocationListQuery(string? UserId, bool IsAdministrator) : IRequest<List<LeaveAllocationDto>>;
}