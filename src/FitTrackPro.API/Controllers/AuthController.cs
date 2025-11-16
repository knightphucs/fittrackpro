namespace FitTrackPro.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using FitTrackPro.Application.Features.Users.Commands.Register;
using FitTrackPro.Application.Features.Users.Commands.Login;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="command">Registration details</param>
    /// <returns>Authentication tokens</returns>
    /// <response code="200">User registered successfully</response>
    /// <response code="400">Invalid request or user already exists</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        _logger.LogInformation("User registration attempt for email: {Email}", command.Email);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                "User registration failed for email: {Email}. Reason: {Reason}",
                command.Email,
                result.Error);
            return BadRequest(new { error = result.Error });
        }

        _logger.LogInformation("User registered successfully: {UserId}", result.Value.UserId);

        return Ok(result.Value);
    }

    /// <summary>
    /// Login user
    /// </summary>
    /// <param name="command">Login credentials</param>
    /// <returns>Authentication tokens</returns>
    /// <response code="200">Login successful</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        _logger.LogInformation("Login attempt for email: {Email}", command.Email);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Login failed for email: {Email}", command.Email);
            return Unauthorized(new { error = "Invalid credentials" });
        }

        _logger.LogInformation("User logged in successfully: {UserId}", result.Value.UserId);

        return Ok(result.Value);
    }
}