namespace FitTrackPro.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using FitTrackPro.Application.Features.Users.Commands.Register;
using FitTrackPro.Application.Features.Users.Commands.Login;
using Microsoft.AspNetCore.Authorization;
using FitTrackPro.Application.Features.Users.Commands.RefreshToken;
using FitTrackPro.Application.Features.Users.Commands.PasswordReset;
using FitTrackPro.Application.Features.Users.Commands.ConfirmEmail;
using System.Security.Claims;
using FitTrackPro.Application.Features.Users.Commands.LogOut;

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
    /// Confirm user email
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="token">Confirmation token</param>
    /// <returns>No content</returns>
    [HttpGet("confirm-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
    {
        var command = new ConfirmEmailCommand(email, token);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = result.Value });    
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
            return Unauthorized(new { error = result.Error });
        }

        _logger.LogInformation("User logged in successfully: {UserId}", result.Value.UserId);

        return Ok(result.Value);
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    /// <param name="command">Refresh token details</param>
    /// <returns>New authentication tokens</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Request password reset link (Forgot Password)
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] RequestPasswordResetCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { message = result.Value });
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = result.Value });
    }

    /// <summary>
    /// Logout user (invalidate specific refresh token)
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
    {
        await _mediator.Send(command);
        
        return Ok(new { message = "Logged out successfully." });
    }
}
