using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR.LeaveManagement.Application.Features.LeaveRequest.Shared;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Identity;


namespace HR.LeaveManagement.Application.Features.LeaveRequest.Shared
{
    public class BaseLeaveRequestValidator : AbstractValidator<BaseLeaveRequest>
    {
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly IUserService _userService;

        public BaseLeaveRequestValidator(ILeaveTypeRepository leaveTypeRepository, IUserService userService)
        {
            _leaveTypeRepository = leaveTypeRepository;
            _userService = userService;

            RuleFor(p => p.StartDate)
                .LessThanOrEqualTo(p => p.EndDate).WithMessage("{PropertyName} must be before {ComparisonProperty}");

            RuleFor(p => p.EndDate)
                .GreaterThanOrEqualTo(p => p.StartDate).WithMessage("{PropertyName} must be after {ComparisonProperty}");

            RuleFor(p => p.LeaveTypeId)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .GreaterThan(0)
                .MustAsync(LeaveTypeMustExist)
                .WithMessage("{PropertyName} does not exist.");

            RuleFor(p => p.RequestingEmployeeId)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MustAsync(EmployeeMustExist)
                .WithMessage("{PropertyName} does not exist.");
        }

        private async Task<bool> LeaveTypeMustExist(int id, CancellationToken token)
        {
            var leaveType = await _leaveTypeRepository.GetByIdAsync(id);
            return leaveType != null;
        }

        private async Task<bool> EmployeeMustExist(string id, CancellationToken token)
        {
            var employee = await _userService.GetEmployee(id);
            return employee != null;
        }
    }
}