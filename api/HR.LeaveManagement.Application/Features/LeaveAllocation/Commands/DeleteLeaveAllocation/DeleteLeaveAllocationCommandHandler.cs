using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Features.LeaveAllocation.Commands.DeleteLeaveAllocation;


namespace HR.LeaveManagement.Application.Features.LeaveAllocation.Commands.DeleteLeaveAllocation
{
    public class DeleteLeaveAllocationCommandHandler : IRequestHandler<DeleteLeaveAllocationCommand, Unit>
    {
        private readonly ILeaveAllocationRepository _leaveAllocationRepository;
        private readonly ILeaveRequestRepository _leaveRequestRepository;

        public DeleteLeaveAllocationCommandHandler(
            ILeaveAllocationRepository leaveAllocationRepository,
            ILeaveRequestRepository leaveRequestRepository)
        {
            _leaveAllocationRepository = leaveAllocationRepository;
            _leaveRequestRepository = leaveRequestRepository;
        }

        public async Task<Unit> Handle(DeleteLeaveAllocationCommand request, CancellationToken cancellationToken)
        {
            var leaveAllocationToDelete = await _leaveAllocationRepository.GetByIdAsync(request.Id);

            if (leaveAllocationToDelete == null)
                throw new NotFoundException(nameof(LeaveAllocation), request.Id);

            var relatedLeaveRequests = await _leaveRequestRepository.GetLeaveRequestsWithDetails(leaveAllocationToDelete.EmployeeId);
            var hasRelatedLeaveRequests = relatedLeaveRequests.Any(lr => lr.LeaveTypeId == leaveAllocationToDelete.LeaveTypeId);

            if (hasRelatedLeaveRequests)
            {
                throw new BadRequestException("無法刪除，因為該員工已有相關的請假申請紀錄");
            }

            await _leaveAllocationRepository.DeleteAsync(leaveAllocationToDelete);

            return Unit.Value;
        }
    }
}