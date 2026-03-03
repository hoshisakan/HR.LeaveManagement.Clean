using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Contracts.Logging;
using HR.LeaveManagement.Application.Features.LeaveRequest.Queries.GetLeaveRequestDetail;


namespace HR.LeaveManagement.Application.Features.LeaveRequest.Queries.GetLeaveRequestDetail
{
    public class GetLeaveRequestDetailQueryHandler : IRequestHandler<GetLeaveRequestDetailQuery, LeaveRequestDetailsDto>
    {
        private readonly IMapper _mapper;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IAppLogger<GetLeaveRequestDetailQueryHandler> _logger;

        public GetLeaveRequestDetailQueryHandler(ILeaveRequestRepository leaveRequestRepository, IMapper mapper, IAppLogger<GetLeaveRequestDetailQueryHandler> logger)
        {
            _mapper = mapper;
            _leaveRequestRepository = leaveRequestRepository;
            _logger = logger;
        }

        public async Task<LeaveRequestDetailsDto> Handle(GetLeaveRequestDetailQuery request, CancellationToken cancellationToken)
        {
            var leaveRequest = await _leaveRequestRepository.GetLeaveRequestWithDetails(request.Id);
            return _mapper.Map<LeaveRequestDetailsDto>(leaveRequest);
        }
    }
}