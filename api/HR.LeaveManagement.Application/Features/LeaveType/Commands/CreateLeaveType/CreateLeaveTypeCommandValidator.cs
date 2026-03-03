using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR.LeaveManagement.Application.Contracts.Persistence;


namespace HR.LeaveManagement.Application.Features.LeaveType.Commands.CreateLeaveType
{
    public class CreateLeaveTypeCommandValidator : AbstractValidator<CreateLeaveTypeCommand>
    {
        private readonly ILeaveTypeRepository _leaveTypeRepository;

        public CreateLeaveTypeCommandValidator(ILeaveTypeRepository leaveTypeRepository)
        {
            _leaveTypeRepository = leaveTypeRepository;

            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");
        
            RuleFor(p => p.DefaultDays)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .LessThan(100).WithMessage("{PropertyName} cannot exceed 100.")
                .GreaterThan(1).WithMessage("{PropertyName} cannot be less than 1.");

            RuleFor(q => q)
                .MustAsync(LeaveTypeNameUnique)
                .WithMessage("Leave type already exists.");
        }

        private async Task<bool> LeaveTypeNameUnique(CreateLeaveTypeCommand command, CancellationToken token)
        {
            return await _leaveTypeRepository.IsLeaveTypeUnique(command.Name);
        }
    }
}