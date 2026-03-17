using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.ChangeLeaveRequestApproval;
using HR.LeaveManagement.Application.Contracts.Logging;
using HR.LeaveManagement.Application.Contracts.Email;
using HR.LeaveManagement.Application.Models.Email;


namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.ChangeLeaveRequestApproval
{
    public class ChangeLeaveRequestApprovalCommandHandler : IRequestHandler<ChangeLeaveRequestApprovalCommand, Unit>
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly ILeaveAllocationRepository _leaveAllocationRepository;
        private readonly IAppLogger<ChangeLeaveRequestApprovalCommandHandler> _logger;
        private readonly IEmailSender _emailSender;

        public ChangeLeaveRequestApprovalCommandHandler(
            ILeaveRequestRepository leaveRequestRepository,
            ILeaveTypeRepository leaveTypeRepository,
            ILeaveAllocationRepository leaveAllocationRepository,
            IAppLogger<ChangeLeaveRequestApprovalCommandHandler> logger,
            IEmailSender emailSender)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _leaveTypeRepository = leaveTypeRepository;
            _leaveAllocationRepository = leaveAllocationRepository;
            _logger = logger;
            _emailSender = emailSender;
        }
        
        public async Task<Unit> Handle(ChangeLeaveRequestApprovalCommand request, CancellationToken cancellationToken)
        {
            var leaveRequest = await _leaveRequestRepository.GetByIdAsync(request.Id);

            if (leaveRequest == null)
                throw new NotFoundException(nameof(LeaveRequest), request.Id);
            
            if  (leaveRequest.Cancelled)
                throw new BadRequestException("This leave request has been cancelled.");

            // 計算本次請假天數（含首尾）
            var daysRequested = (int)(leaveRequest.EndDate.Date - leaveRequest.StartDate.Date).TotalDays + 1;
            if (daysRequested <= 0)
                throw new BadRequestException("Invalid leave request dates.");

            // 讀取員工該假別的配額
            var allocation = await _leaveAllocationRepository.GetUserAllocations(
                leaveRequest.RequestingEmployeeId,
                leaveRequest.LeaveTypeId);

            if (allocation == null)
                throw new BadRequestException("No leave allocation found for this employee and leave type.");

            var wasApproved = leaveRequest.Approved == true;
            var willBeApproved = request.Approved;

            // 從「未核准」變成「核准」：需要檢查餘額並扣除
            if (!wasApproved && willBeApproved)
            {
                var projectedUsed = allocation.UsedDays + daysRequested;
                if (projectedUsed > allocation.NumberOfDays)
                    throw new BadRequestException("Insufficient leave balance for this request.");

                allocation.UsedDays = projectedUsed;
                await _leaveAllocationRepository.UpdateAsync(allocation);
            }

            // 從「已核准」變成「未核准 / 駁回」：需要把天數加回配額
            if (wasApproved && !willBeApproved)
            {
                allocation.UsedDays -= daysRequested;
                if (allocation.UsedDays < 0)
                    allocation.UsedDays = 0;

                await _leaveAllocationRepository.UpdateAsync(allocation);
            }

            leaveRequest.Approved = request.Approved;
            await _leaveRequestRepository.UpdateAsync(leaveRequest);

            try {
                var email = new EmailMessage
                {
                    To = string.Empty, // Get the employee's email from the leaveRequest
                    Subject = "Leave Request Approved",
                    Body = $"Your leave request for {leaveRequest.StartDate:D} to {leaveRequest.EndDate:D} has been approved successfully."
                };
                await _emailSender.SendEmail(email);
            } catch (Exception ex) {
                _logger.LogWarning(ex.Message);
            }

            return Unit.Value;
        }
    }
}