using System.Security.Claims;
using FitTrackPro.Application.Features.DataImport.Commands.ImportFoods;
using FitTrackPro.Application.Features.DataImport.Queries.GetFoodImport;
using FitTrackPro.Application.Features.Export.Queries.ExportData;
using FitTrackPro.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitTrackPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DataController : ControllerBase
{
    private readonly IMediator _mediator;

    public DataController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Export user data to CSV
    /// </summary>
    /// <param name="type">Export type: progress, meallogs, or full</param>
    /// <param name="startDate">Optional start date</param>
    /// <param name="endDate">Optional end date</param>
    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportData(
        [FromQuery] string type = "full",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = GetCurrentUserId();

        var exportType = type.ToLower() switch
        {
            "progress" => ExportType.Progress,
            "meallogs" => ExportType.MealLogs,
            "nutritionreport" => ExportType.NutritionReport,
            "full" => ExportType.Full,
            _ => ExportType.Full
        };

        var query = new ExportDataQuery(
            userId,
            exportType,
            startDate,
            endDate);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var fileName = $"fittrack_export_{type}_{DateTime.UtcNow:yyyyMMdd}.csv";
        
        return File(result.Value, "text/csv", fileName);
    }

    /// <summary>
    /// Import foods from CSV
    /// </summary>
    [HttpPost("import/foods")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportFoods([FromForm] IFormFile file)
    {
        if (file == null || !file.FileName.EndsWith(".csv"))
            return BadRequest("Please upload a valid CSV file.");

        var userId = GetCurrentUserId();

        using var stream = file.OpenReadStream();
        var command = new ImportFoodsCommand(userId, stream);
        
        var result = await _mediator.Send(command);

        if (!result.IsSuccess) return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    /// <summary>
    /// Download sample food import template
    /// </summary>
    [HttpGet("import/foods/template")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadFoodImportTemplate()
    {
        var query = new GetFoodImportTemplateQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess) 
            return BadRequest(result.Error);

        return File(result.Value, "text/csv", "food-import-template.csv");
    }
}