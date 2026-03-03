using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Contracts.Logging;
using HR.LeaveManagement.Application.Features.LeaveRequest.Queries.GetLeaveRequestList;


namespace HR.LeaveManagement.Application.Features.LeaveRequest.Queries.GetLeaveRequestList
{
    public class GetLeaveRequestListQueryHandler : IRequestHandler<GetLeaveRequestListQuery, List<LeaveRequestListDto>>
    {
        private readonly IMapper _mapper;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IAppLogger<GetLeaveRequestListQueryHandler> _logger;

        public GetLeaveRequestListQueryHandler(ILeaveRequestRepository leaveRequestRepository, IMapper mapper, IAppLogger<GetLeaveRequestListQueryHandler> logger)
        {
            _mapper = mapper;
            _leaveRequestRepository = leaveRequestRepository;
            _logger = logger;
        }

        public async Task<List<LeaveRequestListDto>> Handle(GetLeaveRequestListQuery request, CancellationToken cancellationToken)
        {
            var leaveRequests = await _leaveRequestRepository.GetLeaveRequestsWithDetails();
            return _mapper.Map<List<LeaveRequestListDto>>(leaveRequests);
        }
    }
}