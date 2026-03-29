namespace FitTrackPro.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitTrackPro.Application.Features.Foods.Commands.RebuildFoodsIndex;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Features.Workouts.Commands.RebuildExercisesIndex;

[ApiController]
[Route("api/admin/search-index")]
[Authorize(Policy = "RequireAdminRole")]
public class SearchIndexController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ISearchService _searchService;

    public SearchIndexController(IMediator mediator, ISearchService searchService)
    {
        _mediator = mediator;
        _searchService = searchService;
    }

    /// <summary>
    /// Rebuild Foods Search Index
    /// </summary>
    [HttpPost("rebuild-foods-index")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RebuildFoodsIndex()
    {
        var result = await _mediator.Send(new RebuildFoodsIndexCommand());

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Rebuild foods index successfully" });
    }

    [HttpGet("foods/autocomplete")]
    public async Task<IActionResult> AutocompleteFoods(
        [FromQuery] string q,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(Array.Empty<string>());

        var results = await _searchService.AutocompleteFoodsAsync(q, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Rebuild Exercises Search Index (Sync SQL -> Elastic)
    /// </summary>
    [HttpPost("rebuild-exercises-index")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RebuildExercisesIndex()
    {
        var result = await _mediator.Send(new RebuildExercisesIndexCommand());

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Rebuild exercises index successfully" });
    }
}
