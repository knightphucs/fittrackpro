namespace FitTrackPro.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitTrackPro.Application.Features.Workouts.Queries.SearchExercises;
using MailKit.Search;
using FitTrackPro.Domain.Enums;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExercisesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExercisesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search exercises
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchExercises(
        [FromQuery] string? searchTerm,
        [FromQuery] ExerciseCategory? category,
        [FromQuery] MuscleGroup? muscleGroup,
        [FromQuery] EquipmentType? equipment,
        [FromQuery] DifficultyLevel? difficulty,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new SearchExercisesQuery
        {
            SearchTerm = searchTerm,
            Category = category,
            MuscleGroup = muscleGroup,
            Equipment = equipment,
            Difficulty = difficulty,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get exercise categories
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCategories()
    {
        var categories = Enum.GetNames(typeof(FitTrackPro.Domain.Enums.ExerciseCategory));
        return Ok(categories);
    }

    /// <summary>
    /// Get muscle groups
    /// </summary>
    [HttpGet("muscle-groups")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetMuscleGroups()
    {
        var muscleGroups = Enum.GetNames(typeof(FitTrackPro.Domain.Enums.MuscleGroup));
        return Ok(muscleGroups);
    }

    /// <summary>
    /// Get equipment types
    /// </summary>
    [HttpGet("equipment-types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetEquipmentTypes()
    {
        var equipmentTypes = Enum.GetNames(typeof(FitTrackPro.Domain.Enums.EquipmentType));
        return Ok(equipmentTypes);
    }
}
