using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.DeleteLeaveRequest;
using HR.LeaveManagement.Application.Contracts.Logging;

namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.DeleteLeaveRequest
{
    public class DeleteLeaveRequestCommandHandler : IRequestHandler<DeleteLeaveRequestCommand, Unit>
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IAppLogger<DeleteLeaveRequestCommandHandler> _logger;

        public DeleteLeaveRequestCommandHandler(ILeaveRequestRepository leaveRequestRepository, IAppLogger<DeleteLeaveRequestCommandHandler> logger)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _logger = logger;
        }
        
        public async Task<Unit> Handle(DeleteLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            var leaveRequestToDelete = await _leaveRequestRepository.GetByIdAsync(request.Id);

            if (leaveRequestToDelete == null)
                throw new NotFoundException(nameof(LeaveRequest), request.Id);

            await _leaveRequestRepository.DeleteAsync(leaveRequestToDelete);

            return Unit.Value;
        }
    }
}