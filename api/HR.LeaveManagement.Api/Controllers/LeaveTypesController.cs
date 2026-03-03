using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using HR.LeaveManagement.Application.Features.LeaveType.Queries.GetAllLeaveTypes;
using HR.LeaveManagement.Application.Features.LeaveType.Queries.GetLeaveTypeDetails;
using HR.LeaveManagement.Application.Features.LeaveType.Commands.CreateLeaveType;
using HR.LeaveManagement.Application.Features.LeaveType.Commands.UpdateLeaveType;
using HR.LeaveManagement.Application.Features.LeaveType.Commands.DeleteLeaveType;


namespace HR.LeaveManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveTypesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LeaveTypesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<LeaveTypeDto>>> Get()
        {
            var leaveTypes = await _mediator.Send(new GetLeaveTypesQuery());
            return Ok(leaveTypes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveTypeDetailDto>> Get(int id)
        {
            var leaveType = await _mediator.Send(new GetLeaveTypeDetailsQuery { Id = id });
            return Ok(leaveType);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Post([FromBody] CreateLeaveTypeCommand createLeaveTypeCommand)
        {
            var result = await _mediator.Send(createLeaveTypeCommand);
            return CreatedAtAction(nameof(Get), new { id = result }, new { id = result });
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Unit>> Put(int id, [FromBody] UpdateLeaveTypeCommand updateLeaveTypeCommand)
        {
            updateLeaveTypeCommand.Id = id;
            await _mediator.Send(updateLeaveTypeCommand);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Unit>> Delete(int id)
        {
            await _mediator.Send(new DeleteLeaveTypeCommand { Id = id });
            return NoContent();
        }
    }
}