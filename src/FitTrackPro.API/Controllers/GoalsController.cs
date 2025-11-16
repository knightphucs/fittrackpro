namespace FitTrackPro.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FitTrackPro.Application.Features.Goals.Commands.SetGoal;
using FitTrackPro.Application.Features.Goals.Queries.GetCurrentGoal;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoalsController : ControllerBase
{
    private readonly IMediator _mediator;
    public GoalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Set or update fitness goal
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetGoal([FromBody] SetGoalCommand command)
    {
        var userId = GetCurrentUserId();
        var commandWithUserId = command with { UserId = userId };

        var result = await _mediator.Send(commandWithUserId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get current active goal
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentGoal(Guid userId)
    {
        userId = GetCurrentUserId();
        var query = new GetCurrentGoalQuery(userId);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}