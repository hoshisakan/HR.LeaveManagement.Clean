using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.UpdateLeaveRequest;
using HR.LeaveManagement.Application.Features.LeaveRequest.Shared;
using HR.LeaveManagement.Application.Contracts.Persistence;


namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.UpdateLeaveRequest
{
    public class UpdateLeaveRequestCommandValidator : AbstractValidator<UpdateLeaveRequestCommand>
    {
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly ILeaveRequestRepository _leaveRequestRepository;

        public UpdateLeaveRequestCommandValidator(ILeaveTypeRepository leaveTypeRepository, ILeaveRequestRepository leaveRequestRepository)
        {
            _leaveTypeRepository = leaveTypeRepository;
            _leaveRequestRepository = leaveRequestRepository;

            // Validate the base leave request properties
            Include(new BaseLeaveRequestValidator(leaveTypeRepository));

            RuleFor(p => p.Id)
                .NotNull()
                .MustAsync(LeaveRequestMustExist)
                .WithMessage("{PropertyName} does not exist.");
        }

        private async Task<bool> LeaveRequestMustExist(int id, CancellationToken token)
        {
            var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id);
            return leaveRequest != null;
        }
    }
}