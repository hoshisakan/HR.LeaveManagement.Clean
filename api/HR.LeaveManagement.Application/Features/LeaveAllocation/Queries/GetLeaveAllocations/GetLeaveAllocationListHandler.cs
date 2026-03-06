using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Contracts.Logging;


namespace HR.LeaveManagement.Application.Features.LeaveAllocation.Queries.GetLeaveAllocations
{
    public class GetLeaveAllocationListHandler : IRequestHandler<GetLeaveAllocationListQuery, List<LeaveAllocationDto>>
    {
        private readonly IMapper _mapper;
        private readonly ILeaveAllocationRepository _leaveAllocationRepository;
        private readonly IAppLogger<GetLeaveAllocationListHandler> _logger;

        public GetLeaveAllocationListHandler(ILeaveAllocationRepository leaveAllocationRepository, IMapper mapper, IAppLogger<GetLeaveAllocationListHandler> logger)
        {
            _mapper = mapper;
            _leaveAllocationRepository = leaveAllocationRepository;
            _logger = logger;
        }

        public async Task<List<LeaveAllocationDto>> Handle(GetLeaveAllocationListQuery request, CancellationToken cancellationToken)
        {
            var leaveAllocations = request.IsAdministrator
                ? await _leaveAllocationRepository.GetLeaveAllocationsWithDetails()
                : await _leaveAllocationRepository.GetLeaveAllocationsWithDetails(request.UserId!);
            return _mapper.Map<List<LeaveAllocationDto>>(leaveAllocations);
        }
    }
}