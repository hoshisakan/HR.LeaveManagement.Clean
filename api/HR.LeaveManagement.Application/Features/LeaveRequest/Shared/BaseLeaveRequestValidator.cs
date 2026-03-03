using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR.LeaveManagement.Application.Features.LeaveRequest.Shared;
using HR.LeaveManagement.Application.Contracts.Persistence;


namespace HR.LeaveManagement.Application.Features.LeaveRequest.Shared
{
    public class BaseLeaveRequestValidator : AbstractValidator<BaseLeaveRequest>
    {
        private readonly ILeaveTypeRepository _leaveTypeRepository;

        public BaseLeaveRequestValidator(ILeaveTypeRepository leaveTypeRepository)
        {
            _leaveTypeRepository = leaveTypeRepository;

            RuleFor(p => p.StartDate)
                .LessThan(p => p.EndDate).WithMessage("{PropertyName} must be before {ComparisonProperty}");

            RuleFor(p => p.EndDate)
                .GreaterThan(p => p.StartDate).WithMessage("{PropertyName} must be after {ComparisonProperty}");

            RuleFor(p => p.LeaveTypeId)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .GreaterThan(0)
                .MustAsync(LeaveTypeMustExist)
                .WithMessage("{PropertyName} does not exist.");

            RuleFor(p => p.RequestingEmployeeId)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");
        }

        private async Task<bool> LeaveTypeMustExist(int id, CancellationToken token)
        {
            var leaveType = await _leaveTypeRepository.GetByIdAsync(id);
            return leaveType != null;
        }
    }
}