namespace FitTrackPro.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitTrackPro.Application.Features.Foods.Queries.SearchFoods;
using FitTrackPro.Application.Features.Foods.Queries.GetFoodById;
using FitTrackPro.Application.Features.Foods.Queries.GetCategories;
using System.Security.Claims;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Foods.DTOs;
using FitTrackPro.Application.Features.Foods.Commands.CreateFood;
using FitTrackPro.Application.Features.Foods.Commands.UpdateFood;
using FitTrackPro.Application.Features.Foods.Commands.DeleteFood;
using FitTrackPro.Application.Features.Foods.Commands.RecognizeFood;
using FitTrackPro.Application.Features.Foods.Commands.TrainFoodModel;
using FitTrackPro.Domain.Constants;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FoodsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FoodsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search foods with pagination
    /// </summary>
    /// <param name="searchTerm">Search by name (Vietnamese or English)</param>
    /// <param name="category">Filter by category</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchFoods([FromQuery] AdvancedSearchFoodsQuery query)
    {
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get food by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFoodById(Guid id)
    {
        var query = new GetFoodByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get all food categories
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        var query = new GetCategoriesQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new food item
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<FoodDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromForm] CreateFoodCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Update an existing food item
    /// </summary>
    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<FoodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateFoodCommand command)
    {
        if (id != command.FoodId)
        {
            return BadRequest(new { error = "Route id does not match payload." });
        }

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            var isNotFound = result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true;
            return isNotFound
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete a food item
    /// </summary>
    /// <param name="id">Food ID</param>
    /// <returns></returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteFoodCommand(id);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            var isNotFound = result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true;
            return isNotFound
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
        }
        return Ok(result.Value);
    }

    /// <summary>
    /// Helper method to get current user's ID from claims
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user identity.");
        }
        return userId;
    }

    /// <summary>
    /// Nhận diện món ăn từ ảnh upload
    /// POST /api/foods/recognize
    /// </summary>
    [HttpPost("recognize")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RecognizeFood(
        [FromForm] IFormFile image,
        CancellationToken cancellationToken)
    {
        var command = new RecognizeFoodCommand(image);
        var result  = await _mediator.Send(command, cancellationToken);
 
        if (!result.IsSuccess)
        {
            if (result.Error.Contains("chưa sẵn sàng"))
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = result.Error });
 
            return BadRequest(new { error = result.Error });
        }
 
        return Ok(result.Value);
    }
 
    /// <summary>
    /// [ADMIN] Trigger training model
    /// POST /api/foods/train-model
    /// </summary>
    [HttpPost("train-model")]
    [Authorize(Roles = Roles.Administrator)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TrainModel(CancellationToken cancellationToken)
    {
        var command = new TrainFoodModelCommand();
        var result  = await _mediator.Send(command, cancellationToken);
 
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
 
        return Ok(result.Value);
    }
}
