namespace FitTrackPro.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FitTrackPro.Application.Features.Workouts.Commands.DeleteWorkout;
using FitTrackPro.Application.Features.Workouts.Queries.GetPersonalRecords;
using FitTrackPro.Application.Features.Workouts.Queries.GetWorkoutSummary;
using FitTrackPro.Application.Features.Workouts.Queries.GetActiveWorkout;
using FitTrackPro.Application.Features.Workouts.Queries.GetWorkoutHistory;
using FitTrackPro.Application.Features.Workouts.Commands.CompleteWorkout;
using FitTrackPro.Application.Features.Workouts.Commands.LogExercise;
using FitTrackPro.Application.Features.Workouts.Commands.StartWorkout;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkoutsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkoutsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Start a new workout session
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartWorkout([FromBody] StartWorkoutCommand command)
    {
        var userId = GetCurrentUserId();
        var commandWithUserId = command with { UserId = userId };

        var result = await _mediator.Send(commandWithUserId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Log an exercise to current workout
    /// </summary>
    [HttpPost("{workoutId:guid}/exercises")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogExercise(
        Guid workoutId,
        [FromBody] LogExerciseCommand command)
    {
        var userId = GetCurrentUserId();
        var commandWithUserId = command with 
        { 
            UserId = userId,
            WorkoutSessionId = workoutId
        };

        var result = await _mediator.Send(commandWithUserId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Complete a workout session
    /// </summary>
    [HttpPost("{workoutId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteWorkout(
        Guid workoutId,
        [FromBody] CompleteWorkoutCommand command)
    {
        var userId = GetCurrentUserId();
        var commandWithUserId = command with 
        { 
            UserId = userId,
            WorkoutSessionId = workoutId
        };

        var result = await _mediator.Send(commandWithUserId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get workout history
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkoutHistory(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var query = new GetWorkoutHistoryQuery(
            userId, 
            startDate, 
            endDate, 
            pageNumber, 
            pageSize);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get active workout session
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveWorkout()
    {
        var userId = GetCurrentUserId();
        var query = new GetActiveWorkoutQuery(userId);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get workout summary/statistics
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkoutSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = GetCurrentUserId();
        var query = new GetWorkoutSummaryQuery(userId, startDate, endDate);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get personal records
    /// </summary>
    [HttpGet("records")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPersonalRecords(
        [FromQuery] Guid? exerciseId = null)
    {
        var userId = GetCurrentUserId();
        var query = new GetPersonalRecordsQuery(userId, exerciseId);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete a workout session
    /// </summary>
    [HttpDelete("{workoutId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWorkout(Guid workoutId)
    {
        var userId = GetCurrentUserId();
        var command = new DeleteWorkoutCommand
        {
            UserId = userId,
            WorkoutSessionId = workoutId
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }
}
