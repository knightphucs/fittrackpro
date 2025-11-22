namespace FitTrackPro.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FitTrackPro.Application.Features.MealLogs.Commands.LogMeal;
using FitTrackPro.Application.Features.MealLogs.Commands.DeleteMealLog;
using FitTrackPro.Application.Features.MealLogs.Queries.GetDailyMeals;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MealLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MealLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Log a meal
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogMeal([FromBody] LogMealCommand command)
    {
        var userId = GetCurrentUserId();
        var commandWithUserId = command with { UserId = userId };

        var result = await _mediator.Send(commandWithUserId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get daily meals
    /// </summary>
    /// <param name="date">Date in format yyyy-MM-dd (optional, defaults to today)</param>
    [HttpGet("daily")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyMeals([FromQuery] DateTime? date)
    {
        var userId = GetCurrentUserId();
        var targetDate = date ?? DateTime.Today;

        var query = new GetDailyMealsQuery(userId, targetDate);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete a meal log
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMealLog(Guid id)
    {
        var userId = GetCurrentUserId();
        var command = new DeleteMealLogCommand(userId, id);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }
}
