using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HR.LeaveManagement.Application.Features.LeaveRequest.Queries.GetLeaveRequestList;
using HR.LeaveManagement.Application.Features.LeaveRequest.Queries.GetLeaveRequestDetail;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.CreateLeaveRequest;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.UpdateLeaveRequest;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.DeleteLeaveRequest;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.ChangeLeaveRequestApproval;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.CancelLeaveRequest;
using HR.LeaveManagement.Domain;


namespace HR.LeaveManagement.Application.MappingProfiles
{
    public class LeaveRequestProfile : Profile
    {
        public LeaveRequestProfile()
        {
            CreateMap<LeaveRequestListDto, LeaveRequest>().ReverseMap();
            CreateMap<LeaveRequestDetailsDto, LeaveRequest>().ReverseMap();
            CreateMap<CreateLeaveRequestCommand, LeaveRequest>();
            CreateMap<UpdateLeaveRequestCommand, LeaveRequest>();
            CreateMap<DeleteLeaveRequestCommand, LeaveRequest>();
            CreateMap<ChangeLeaveRequestApprovalCommand, LeaveRequest>();
            CreateMap<CancelLeaveRequestCommand, LeaveRequest>();
        }
    }
}