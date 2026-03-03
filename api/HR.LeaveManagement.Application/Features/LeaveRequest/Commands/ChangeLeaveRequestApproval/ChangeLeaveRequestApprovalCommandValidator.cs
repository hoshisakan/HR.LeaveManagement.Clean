using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.ChangeLeaveRequestApproval;
using HR.LeaveManagement.Application.Contracts.Persistence;


namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.ChangeLeaveRequestApproval
{
    public class ChangeLeaveRequestApprovalCommandValidator : AbstractValidator<ChangeLeaveRequestApprovalCommand>
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;

        public ChangeLeaveRequestApprovalCommandValidator(ILeaveRequestRepository leaveRequestRepository)
        {
            _leaveRequestRepository = leaveRequestRepository;

            RuleFor(p => p.Id)
                .NotNull()
                .MustAsync(LeaveRequestMustExist)
                .WithMessage("{PropertyName} does not exist.");

            RuleFor(p => p.Approved)
                .NotNull()
                .WithMessage("{PropertyName} is required.");
        }

        private async Task<bool> LeaveRequestMustExist(int id, CancellationToken token)
        {
            var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id);
            return leaveRequest != null;
        }
    }
}