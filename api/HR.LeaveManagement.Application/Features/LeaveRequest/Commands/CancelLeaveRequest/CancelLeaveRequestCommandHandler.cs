using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.CancelLeaveRequest;
using HR.LeaveManagement.Application.Contracts.Logging;
using HR.LeaveManagement.Application.Contracts.Email;
using HR.LeaveManagement.Application.Models.Email;


namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.CancelLeaveRequest
{
    public class CancelLeaveRequestCommandHandler : IRequestHandler<CancelLeaveRequestCommand, Unit>
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IAppLogger<CancelLeaveRequestCommandHandler> _logger;
        private readonly IEmailSender _emailSender;

        public CancelLeaveRequestCommandHandler(ILeaveRequestRepository leaveRequestRepository, IAppLogger<CancelLeaveRequestCommandHandler> logger, IEmailSender emailSender)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _logger = logger;
            _emailSender = emailSender;
        }

        public async Task<Unit> Handle(CancelLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            var leaveRequest = await _leaveRequestRepository.GetByIdAsync(request.Id);

            if (leaveRequest == null)
                throw new NotFoundException(nameof(LeaveRequest), request.Id);

            if (!request.IsAdministrator && leaveRequest.RequestingEmployeeId != request.UserId)
                throw new NotFoundException(nameof(LeaveRequest), request.Id);

            leaveRequest.Cancelled = true;
            await _leaveRequestRepository.UpdateAsync(leaveRequest);

            try {
                var email = new EmailMessage
                {
                    To = string.Empty, // Get the employee's email from the leaveRequest
                    Subject = "Leave Request Cancelled",
                    Body = $"Your leave request for {leaveRequest.StartDate:D} to {leaveRequest.EndDate:D} has been cancelled successfully."
                };
                await _emailSender.SendEmail(email);
            } catch (Exception ex) {
                _logger.LogWarning(ex.Message);
            }

            return Unit.Value;
        }
    }
}