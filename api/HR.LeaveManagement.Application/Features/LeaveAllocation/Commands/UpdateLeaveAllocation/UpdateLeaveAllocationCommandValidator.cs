using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Features.LeaveAllocation.Commands.UpdateLeaveAllocation;


namespace HR.LeaveManagement.Application.Features.LeaveAllocation.Commands.UpdateLeaveAllocation
{
    public class UpdateLeaveAllocationCommandValidator : AbstractValidator<UpdateLeaveAllocationCommand>
    {
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly ILeaveAllocationRepository _leaveAllocationRepository;

        public UpdateLeaveAllocationCommandValidator(ILeaveTypeRepository leaveTypeRepository, ILeaveAllocationRepository leaveAllocationRepository)
        {
            _leaveTypeRepository = leaveTypeRepository;
            _leaveAllocationRepository = leaveAllocationRepository;

            RuleFor(p => p.Id)
                .NotNull()
                .WithMessage("{PropertyName} is required.")
                .MustAsync(LeaveAllocationMustExist)
                .WithMessage("{PropertyName} does not exist.");
            
            RuleFor(p => p.NumberOfDays)
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0.");

            RuleFor(p => p.LeaveTypeId)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .GreaterThan(0)
                .MustAsync(LeaveTypeMustExist)
                .WithMessage("{PropertyName} does not exist.");

            RuleFor(p => p.Period)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .GreaterThan(0)
                .GreaterThanOrEqualTo(DateTime.Now.Year)
                .WithMessage("{PropertyName} must be greater than or equal to the current year.");

            RuleFor(p => p.EmployeeId)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");
        }

        private async Task<bool> LeaveAllocationMustExist(int id, CancellationToken token)
        {
            var leaveAllocation = await _leaveAllocationRepository.GetByIdAsync(id);
            return leaveAllocation != null;
        }

        private async Task<bool> LeaveTypeMustExist(int id, CancellationToken token)
        {
            var leaveType = await _leaveTypeRepository.GetByIdAsync(id);
            return leaveType != null;
        }
    }
}