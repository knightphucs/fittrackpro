namespace FitTrackPro.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FitTrackPro.Application.Features.Analytics.Queries.GetWeeklyReport;
using FitTrackPro.Application.Features.Analytics.Queries.GetNutritionTrends;
using FitTrackPro.Application.Features.Analytics.Queries.GetGoalPrediction;
using FitTrackPro.Application.Features.Analytics.Queries.GetDashboard;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Get weekly report with achievements and recommendations
    /// </summary>
    /// <param name="startDate">Optional start date (defaults to 7 days ago)</param>
    [HttpGet("weekly-report")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetWeeklyReport([FromQuery] DateTime? startDate = null)
    {
        var userId = GetCurrentUserId();
        var query = new GetWeeklyReportQuery(userId, startDate);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get nutrition trends over a period
    /// </summary>
    /// <param name="days">Number of days to analyze (default: 30, max: 90)</param>
    [HttpGet("nutrition-trends")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNutritionTrends([FromQuery] int days = 30)
    {
        if (days > 90) days = 90;
        if (days < 7) days = 7;

        var userId = GetCurrentUserId();
        var query = new GetNutritionTrendsQuery(userId, days);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get dashboard summary (quick overview)
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] 
    public async Task<IActionResult> GetDashboard()
    {
        var userId = GetCurrentUserId();
        var query = new GetDashboardQuery(userId);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get goal achievement prediction
    /// </summary>
    [HttpGet("goal-prediction")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGoalPrediction()
    {
        var userId = GetCurrentUserId();
        var query = new GetGoalPredictionQuery(userId);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new
        {
            message = "Goal prediction endpoint - implement GetGoalPredictionQuery",
            userId
        });
    }

    /// <summary>
    /// Download weekly report as PDF
    /// </summary>
    [HttpGet("weekly-report/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DownloadWeeklyReportPdf([FromQuery] DateTime? startDate = null)
    {
        var userId = GetCurrentUserId();
        
        // TODO: Implement this using IPdfExportService
        return BadRequest(new { error = "PDF export not yet implemented in handler" });
    }

    /// <summary>
    /// Get monthly summary
    /// </summary>
    /// <param name="year">Year</param>
    /// <param name="month">Month (1-12)</param>
    [HttpGet("monthly-summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMonthlySummary(
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        var userId = GetCurrentUserId();
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;

        if (targetMonth < 1 || targetMonth > 12)
            return BadRequest(new { error = "Month must be between 1 and 12" });

        // TODO: Implement GetMonthlySummaryQuery
        // Similar to weekly report but for a full month

        return Ok(new
        {
            message = "Monthly summary endpoint - implement GetMonthlySummaryQuery",
            userId,
            year = targetYear,
            month = targetMonth
        });
    }
}