using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Features.LeaveAllocation.Commands.CreateLeaveAllocation;
using HR.LeaveManagement.Domain;
using HR.LeaveManagement.Application.Contracts.Logging;
using HR.LeaveManagement.Application.Features.LeaveType.Queries.GetLeaveTypeDetails;


namespace HR.LeaveManagement.Application.Features.LeaveAllocation.Commands.CreateLeaveAllocation
{
    public class CreateLeaveAllocationCommandHandler : IRequestHandler<CreateLeaveAllocationCommand, int>
    {
        private readonly IMapper _mapper;
        private readonly ILeaveAllocationRepository _leaveAllocationRepository;
        private readonly ILeaveTypeRepository _leaveTypeRepository;

        public CreateLeaveAllocationCommandHandler(ILeaveAllocationRepository leaveAllocationRepository, ILeaveTypeRepository leaveTypeRepository, IMapper mapper)
        {
            _mapper = mapper;
            _leaveAllocationRepository = leaveAllocationRepository;
            _leaveTypeRepository = leaveTypeRepository;
        }

        public async Task<int> Handle(CreateLeaveAllocationCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateLeaveAllocationCommandValidator(_leaveTypeRepository, _leaveAllocationRepository);
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.Errors.Any())
                throw new BadRequestException("Invalid Leave allocation", validationResult);

            var leaveType = await _leaveTypeRepository.GetByIdAsync(request.LeaveTypeId);
            if (leaveType == null)
                throw new NotFoundException(nameof(LeaveType), request.LeaveTypeId);

            var leaveAllocation = _mapper.Map<HR.LeaveManagement.Domain.LeaveAllocation>(request);
            leaveAllocation = await _leaveAllocationRepository.CreateAsync(leaveAllocation);

            // If the number of days is 0, set it to the default number of days
            if (leaveAllocation.NumberOfDays == 0)
                leaveAllocation.NumberOfDays = leaveType.DefaultDays;

            return leaveAllocation.Id;
        }
    }
}