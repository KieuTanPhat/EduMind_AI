using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyAI.Application.Features.Auth.Commands;
using StudyAI.Application.Features.Auth.Queries;
using StudyAI.Contracts.Auth;

namespace StudyAI.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new RegisterCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetCurrentUser), routeValues: null, value: response);
    }

    [HttpGet("captcha")]
    [ProducesResponseType(typeof(CaptchaResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CaptchaResponse>> GetCaptcha(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetCaptchaQuery(), cancellationToken));

    [HttpPost("resend-verification")]
    public async Task<ActionResult<ResendVerificationResponse>> ResendVerification(ResendEmailVerificationRequest request, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ResendEmailVerificationCommand(request), cancellationToken));

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token, CancellationToken cancellationToken)
    {
        await _sender.Send(new VerifyEmailCommand(token), cancellationToken);
        return Ok(new { message = "Email verified successfully." });
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new LoginCommand(request), cancellationToken));
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new RefreshTokenCommand(request), cancellationToken));
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new LogoutCommand(request), cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        return Ok(await _sender.Send(new GetCurrentUserQuery(userId), cancellationToken));
    }
}
