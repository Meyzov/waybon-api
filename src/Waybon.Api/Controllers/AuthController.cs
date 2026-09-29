using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Waybon.Api.RateLimiting;
using Waybon.Application.Auth.Abstractions;
using Waybon.Application.Auth.Dtos;

namespace Waybon.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    // ===================================
    // POST: api/v1/auth/register
    // ===================================

    [HttpPost("register")]
    [EnableRateLimiting(RateLimitPolicies.Register)]
    public async Task<ActionResult<AuthUserResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.RegisterAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }


    // ===================================
    // POST: api/v1/auth/login
    // ===================================

    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }


    // ===================================
    // POST: api/v1/auth/verification-code
    // ===================================

    [HttpPost("verification-code")]
    [EnableRateLimiting(RateLimitPolicies.Verification)]
    public async Task<IActionResult> SendVerificationCode(SendVerificationCodeRequest request, CancellationToken cancellationToken)
    {
        await authService.SendVerificationCodeAsync(request, cancellationToken);
        return NoContent();
    }


    // ===================================
    // POST: api/v1/auth/verify-email
    // ===================================

    [HttpPost("verify-email")]
    [EnableRateLimiting(RateLimitPolicies.Verification)]
    public async Task<ActionResult<LoginResponse>> VerifyEmail(VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.VerifyEmailAsync(request, cancellationToken);
        return Ok(response);
    }
}