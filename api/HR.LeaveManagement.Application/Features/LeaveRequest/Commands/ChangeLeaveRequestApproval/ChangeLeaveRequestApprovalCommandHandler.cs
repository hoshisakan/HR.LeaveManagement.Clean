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
        private readonly IAppLogger<ChangeLeaveRequestApprovalCommandHandler> _logger;
        private readonly IEmailSender _emailSender;

        public ChangeLeaveRequestApprovalCommandHandler(ILeaveRequestRepository leaveRequestRepository, ILeaveTypeRepository leaveTypeRepository, IAppLogger<ChangeLeaveRequestApprovalCommandHandler> logger, IEmailSender emailSender)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _leaveTypeRepository = leaveTypeRepository;
            _logger = logger;
            _emailSender = emailSender;
        }
        
        public async Task<Unit> Handle(ChangeLeaveRequestApprovalCommand request, CancellationToken cancellationToken)
        {
            var leaveRequest = await _leaveRequestRepository.GetByIdAsync(request.Id);

            if (leaveRequest == null)
                throw new NotFoundException(nameof(LeaveRequest), request.Id);

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