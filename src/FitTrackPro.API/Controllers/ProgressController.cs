namespace FitTrackPro.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FitTrackPro.Application.Features.Progress.Commands.LogWeight;
using FitTrackPro.Application.Features.Progress.Commands.LogMeasurements;
using FitTrackPro.Application.Features.Progress.Commands.UploadProgressPhoto;
using FitTrackPro.Application.Features.Progress.Queries.GetProgressHistory;
using FitTrackPro.Application.Features.Progress.Queries.GetProgressStatistics;
using FitTrackPro.Application.Features.Progress.Queries.GetProgressPhotos;
using FitTrackPro.Application.Features.Progress.Queries.CompareProgress;
using FitTrackPro.Application.Features.Progress.Commands.UpdateProgressPhoto;
using FitTrackPro.Application.Features.Progress.Commands.DeleteProgressPhoto;
using FitTrackPro.Application.Features.Progress.Commands.UploadMultiplePhotos;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProgressController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Log weight (quick entry)
    /// </summary>
    [HttpPost("weight")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogWeight([FromBody] LogWeightCommand command)
    {
        var userId = GetCurrentUserId();
        var commandWithUserId = command with { UserId = userId };

        var result = await _mediator.Send(commandWithUserId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Log complete measurements
    /// </summary>
    [HttpPost("measurements")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogMeasurements([FromBody] LogMeasurementsCommand command)
    {
        var userId = GetCurrentUserId();
        var commandWithUserId = command with { UserId = userId };

        var result = await _mediator.Send(commandWithUserId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Upload progress photo
    /// </summary>
    [HttpPost("photos")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadProgressPhoto(
        [FromForm] IFormFile photo,
        [FromForm] string photoType,
        [FromForm] decimal? weight = null,
        [FromForm] DateTime? takenAt = null,
        [FromForm] string? notes = null)
    {
        var userId = GetCurrentUserId();
        var command = new UploadProgressPhotoCommand
        {
            UserId = userId,
            Photo = photo,
            PhotoType = photoType,
            Weight = weight,
            TakenAt = takenAt,
            Notes = notes
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get progress history
    /// </summary>
    /// <param name="startDate">Optional start date</param>
    /// <param name="endDate">Optional end date</param>
    [HttpGet("history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProgressHistory(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = GetCurrentUserId();
        var query = new GetProgressHistoryQuery(userId, startDate, endDate);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get progress statistics
    /// </summary>
    /// <param name="days">Number of days to analyze (default: 30)</param>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProgressStatistics([FromQuery] int days = 30)
    {
        var userId = GetCurrentUserId();
        var query = new GetProgressStatisticsQuery(userId, days);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get progress photos
    /// </summary>
    /// <param name="photoType">Filter by photo type (Front, Side, Back)</param>
    /// <param name="startDate">Optional start date</param>
    /// <param name="endDate">Optional end date</param>
    [HttpGet("photos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProgressPhotos(
        [FromQuery] string? photoType = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = GetCurrentUserId();
        var query = new GetProgressPhotosQuery(userId, photoType, startDate, endDate);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Update a progress photo
    /// </summary>
    [HttpPut("photos/{id}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateProgressPhotoCommand command)
    {
        command.UserId = GetCurrentUserId();

        command.PhotoId = id;

        // 3. Send to Handler
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }
    
    /// <summary>
    /// Delete a progress photo
    /// </summary>
    [HttpDelete("photos/{photoId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProgressPhoto(Guid photoId)
    {
        var userId = GetCurrentUserId();
        var command = new DeleteProgressPhotoCommand(photoId, userId);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Upload multiple progress photos at once
    /// </summary>
    [HttpPost("photos/multiple")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadMultiplePhotos(
        [FromForm] List<IFormFile> photos,
        [FromForm] List<string> photoTypes,
        [FromForm] decimal? weight = null,
        [FromForm] DateTime? takenAt = null,
        [FromForm] string? notes = null)
    {
        var userId = GetCurrentUserId();
        var command = new UploadMultiplePhotosCommand
        {
            UserId = userId,
            Photos = photos,
            PhotoTypes = photoTypes,
            Weight = weight,
            TakenAt = takenAt,
            Notes = notes
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// 
    /// <summary>
    /// Compare progress between two dates
    /// </summary>
    [HttpGet("compare")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompareProgress(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = GetCurrentUserId();
        var query = new CompareProgressQuery(userId, startDate, endDate);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }
}