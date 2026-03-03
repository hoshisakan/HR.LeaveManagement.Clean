using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Features.LeaveAllocation.Commands.UpdateLeaveAllocation;
using HR.LeaveManagement.Application.Contracts.Logging;


namespace HR.LeaveManagement.Application.Features.LeaveAllocation.Commands.UpdateLeaveAllocation
{
    public class UpdateLeaveAllocationCommandHandler : IRequestHandler<UpdateLeaveAllocationCommand, Unit>
    {
        private readonly IMapper _mapper;
        private readonly ILeaveAllocationRepository _leaveAllocationRepository;
        private readonly ILeaveTypeRepository _leaveTypeRepository;

        public UpdateLeaveAllocationCommandHandler(ILeaveAllocationRepository leaveAllocationRepository, ILeaveTypeRepository leaveTypeRepository, IMapper mapper)
        {
            _mapper = mapper;
            _leaveAllocationRepository = leaveAllocationRepository;
            _leaveTypeRepository = leaveTypeRepository;
        }

        public async Task<Unit> Handle(UpdateLeaveAllocationCommand request, CancellationToken cancellationToken)
        {
            var validator = new UpdateLeaveAllocationCommandValidator(_leaveTypeRepository, _leaveAllocationRepository);
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.Errors.Any())
                throw new BadRequestException("Invalid Leave allocation", validationResult);

            var leaveAllocation = await _leaveAllocationRepository.GetByIdAsync(request.Id);
            
            if (leaveAllocation == null)
                throw new NotFoundException(nameof(LeaveAllocation), request.Id);

            if (leaveAllocation.EmployeeId != request.EmployeeId || leaveAllocation.LeaveTypeId != request.LeaveTypeId)
                throw new BadRequestException("Changing Employee or Leave Type is not allowed during update.");
            
            _mapper.Map(request, leaveAllocation);
            await _leaveAllocationRepository.UpdateAsync(leaveAllocation);

            return Unit.Value;
        }
    }
}