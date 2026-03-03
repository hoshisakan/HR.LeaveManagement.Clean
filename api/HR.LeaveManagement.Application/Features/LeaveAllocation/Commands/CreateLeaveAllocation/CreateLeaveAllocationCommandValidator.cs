using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Features.LeaveAllocation.Commands.CreateLeaveAllocation;


namespace HR.LeaveManagement.Application.Features.LeaveAllocation.Commands.CreateLeaveAllocation
{
    public class CreateLeaveAllocationCommandValidator : AbstractValidator<CreateLeaveAllocationCommand>
    {
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly ILeaveAllocationRepository _leaveAllocationRepository;

        public CreateLeaveAllocationCommandValidator(ILeaveTypeRepository leaveTypeRepository, ILeaveAllocationRepository leaveAllocationRepository)
        {
            _leaveTypeRepository = leaveTypeRepository;
            _leaveAllocationRepository = leaveAllocationRepository;

            RuleFor(p => p.LeaveTypeId)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .GreaterThan(0)
                .MustAsync(LeaveTypeMustExist)
                .WithMessage("{PropertyName} does not exist.");

            RuleFor(p => p.NumberOfDays)
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0.");

            RuleFor(p => p.Period)
                .GreaterThanOrEqualTo(DateTime.Now.Year)
                .WithMessage("{PropertyName} must be greater than or equal to the current year.");

            RuleFor(p => p.EmployeeId)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");

            RuleFor(q => q)
                .MustAsync(LeaveAllocationMustExist)
                .WithMessage("Leave allocation already exists.");
        }

        private async Task<bool> LeaveTypeMustExist(int id, CancellationToken token)
        {
            var leaveType = await _leaveTypeRepository.GetByIdAsync(id);
            return leaveType != null;
        }

        private async Task<bool> LeaveAllocationMustExist(CreateLeaveAllocationCommand command, CancellationToken token)
        {
            var existingAllocation = await _leaveAllocationRepository.LeaveAllocationMustExist(command.EmployeeId, command.LeaveTypeId, command.Period);
            return !existingAllocation;
        }
    }
}