using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using HR.LeaveManagement.Application.Features.LeaveRequest.Queries.GetLeaveRequestList;
using HR.LeaveManagement.Application.Features.LeaveRequest.Queries.GetLeaveRequestDetail;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.CreateLeaveRequest;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.CancelLeaveRequest;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.ChangeLeaveRequestApproval;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.UpdateLeaveRequest;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.DeleteLeaveRequest;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using System.Security.Claims;
using Microsoft.Extensions.Logging;


namespace HR.LeaveManagement.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<LeaveRequestsController> _logger;

        public LeaveRequestsController(IMediator mediator, ILogger<LeaveRequestsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator,Employee")]
        public async Task<ActionResult<List<LeaveRequestListDto>>> Get()
        {
            var userId = User.FindFirst("uid")?.Value;

            if (userId == null)
                return Unauthorized();

            _logger.LogInformation("User {UserId} is requesting leave requests", userId);

            var leaveRequests = await _mediator.Send(new GetLeaveRequestListQuery { UserId = userId });
            return Ok(leaveRequests);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator,Employee")]
        public async Task<ActionResult<LeaveRequestDetailsDto>> Get(int id)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (userId == null)
                return Unauthorized();

            var leaveRequest = await _mediator.Send(
                new GetLeaveRequestDetailQuery
                {
                    Id = id,
                    UserId = userId,
                    IsAdministrator = User.IsInRole("Administrator")
                }
            );
            return Ok(leaveRequest);
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Administrator,Employee")]
        public async Task<ActionResult> Post([FromBody] CreateLeaveRequestCommand createLeaveRequestCommand)
        {
            var userId = User.FindFirst("uid")?.Value;

            if (userId == null)
                return Unauthorized();

            createLeaveRequestCommand.RequestingEmployeeId = userId;
            var result = await _mediator.Send(createLeaveRequestCommand);
            return CreatedAtAction(nameof(Get), new { id = result }, new { id = result });
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Administrator,Employee")]
        public async Task<ActionResult<Unit>> Put(int id, [FromBody] UpdateLeaveRequestCommand updateLeaveRequestCommand)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (userId == null)
                return Unauthorized();

            updateLeaveRequestCommand.Id = id;
            updateLeaveRequestCommand.RequestingEmployeeId = userId;
            await _mediator.Send(updateLeaveRequestCommand);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<Unit>> Delete(int id)
        {
            await _mediator.Send(new DeleteLeaveRequestCommand { Id = id });
            return NoContent();
        }

        [HttpPut("{id}/approve")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<Unit>> Approve(int id, [FromBody] ChangeLeaveRequestApprovalCommand changeLeaveRequestApprovalCommand)
        {
            changeLeaveRequestApprovalCommand.Id = id;
            await _mediator.Send(changeLeaveRequestApprovalCommand);
            return NoContent();
        }

        [HttpPut("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Administrator,Employee")]
        public async Task<ActionResult<Unit>> Cancel(int id)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (userId == null)
                return Unauthorized();

            await _mediator.Send(
                new CancelLeaveRequestCommand {
                    Id = id,
                    UserId = userId,
                    IsAdministrator = User.IsInRole("Administrator")
                }
            );
            return NoContent();
        }
    }
}