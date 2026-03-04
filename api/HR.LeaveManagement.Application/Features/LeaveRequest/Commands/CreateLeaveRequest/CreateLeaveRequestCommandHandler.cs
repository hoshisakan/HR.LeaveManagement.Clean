using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.CreateLeaveRequest;
using HR.LeaveManagement.Application.Contracts.Logging;
using HR.LeaveManagement.Application.Contracts.Email;
using HR.LeaveManagement.Application.Models.Email;
using HR.LeaveManagement.Application.Identity;


namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.CreateLeaveRequest
{
    public class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand, int>
    {
        private readonly IMapper _mapper;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly IAppLogger<CreateLeaveRequestCommandHandler> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IUserService _userService;

        public CreateLeaveRequestCommandHandler(ILeaveRequestRepository leaveRequestRepository, ILeaveTypeRepository leaveTypeRepository, IMapper mapper, IAppLogger<CreateLeaveRequestCommandHandler> logger, IEmailSender emailSender, IUserService userService)
        {
            _mapper = mapper;
            _leaveRequestRepository = leaveRequestRepository;
            _leaveTypeRepository = leaveTypeRepository;
            _logger = logger;
            _emailSender = emailSender;
            _userService = userService;
        }

        public async Task<int> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateLeaveRequestCommandValidator(_leaveTypeRepository, _userService);
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.Errors.Any())
                throw new BadRequestException("Invalid Leave request", validationResult);
        
            var leaveRequest = _mapper.Map<HR.LeaveManagement.Domain.LeaveRequest>(request);
            leaveRequest = await _leaveRequestRepository.CreateAsync(leaveRequest);

            try {
                var email = new EmailMessage
                {
                    To = string.Empty, // Get the employee's email from the leaveRequest
                    Subject = "Leave Request Submitted",
                    Body = $"Your leave request for {request.StartDate:D} to {request.EndDate:D} has been submitted successfully."
                };
                await _emailSender.SendEmail(email);
            } catch (Exception ex) {
                _logger.LogWarning(ex.Message);
            }

            return leaveRequest.Id;
        }
    }
}