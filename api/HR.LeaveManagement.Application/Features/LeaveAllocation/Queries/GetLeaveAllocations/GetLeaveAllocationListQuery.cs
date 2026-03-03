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

    }

    // alternative way to define the query
    // public record GetLeaveAllocationListQuery() : IRequest<List<LeaveAllocationDto>>;
}