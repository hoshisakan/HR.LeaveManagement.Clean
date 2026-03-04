using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.CreateLeaveRequest;
using HR.LeaveManagement.Application.Features.LeaveRequest.Shared;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Identity;


namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.CreateLeaveRequest
{
    public class CreateLeaveRequestCommandValidator : AbstractValidator<CreateLeaveRequestCommand>
    {
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly IUserService _userService;

        public CreateLeaveRequestCommandValidator(ILeaveTypeRepository leaveTypeRepository, IUserService userService)
        {
            _leaveTypeRepository = leaveTypeRepository;
            _userService = userService;

            Include(new BaseLeaveRequestValidator(leaveTypeRepository, userService));
        }
    }
}