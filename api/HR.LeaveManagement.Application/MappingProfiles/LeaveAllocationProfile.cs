using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HR.LeaveManagement.Application.Features.LeaveAllocation.Queries.GetLeaveAllocations;
using HR.LeaveManagement.Application.Features.LeaveAllocation.Queries.GetLeaveAllocationDetails;
using HR.LeaveManagement.Application.Features.LeaveType.Queries.GetLeaveTypeDetails;
using HR.LeaveManagement.Application.Features.LeaveAllocation.Commands.CreateLeaveAllocation;
using HR.LeaveManagement.Application.Features.LeaveAllocation.Commands.UpdateLeaveAllocation;
using HR.LeaveManagement.Domain;


namespace HR.LeaveManagement.Application.MappingProfiles
{
    public class LeaveAllocationProfile : Profile
    {
        public LeaveAllocationProfile()
        {
            CreateMap<LeaveAllocationDto, LeaveAllocation>().ReverseMap();
            CreateMap<LeaveAllocationDetailsDto, LeaveAllocation>().ReverseMap();
            CreateMap<CreateLeaveAllocationCommand, LeaveAllocation>();
            CreateMap<UpdateLeaveAllocationCommand, LeaveAllocation>();
        }
    }
}