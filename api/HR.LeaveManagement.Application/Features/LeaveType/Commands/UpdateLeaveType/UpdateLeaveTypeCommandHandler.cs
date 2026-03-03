using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Contracts.Logging;


namespace HR.LeaveManagement.Application.Features.LeaveType.Commands.UpdateLeaveType
{
    public class UpdateLeaveTypeCommandHandler : IRequestHandler<UpdateLeaveTypeCommand, Unit>
    {
        private readonly IMapper _mapper;
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly IAppLogger<UpdateLeaveTypeCommandHandler> _logger;

        public UpdateLeaveTypeCommandHandler(ILeaveTypeRepository leaveTypeRepository, IMapper mapper, IAppLogger<UpdateLeaveTypeCommandHandler> logger)
        {
            _mapper = mapper;
            _leaveTypeRepository = leaveTypeRepository;
            _logger = logger;
        }
        
        public async Task<Unit> Handle(UpdateLeaveTypeCommand request, CancellationToken cancellationToken)
        {
            var validator = new UpdateLeaveTypeCommandValidator(_leaveTypeRepository);
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.Errors.Any())
            {
                _logger.LogWarning("Validation errors in update request for {0} - {1}", nameof(LeaveType), request.Id);
                throw new BadRequestException("Invalid Leave type", validationResult);
            }

            var leaveType = _mapper.Map<HR.LeaveManagement.Domain.LeaveType>(request);

            await _leaveTypeRepository.UpdateAsync(leaveType);

            return Unit.Value;
        }
    }
}