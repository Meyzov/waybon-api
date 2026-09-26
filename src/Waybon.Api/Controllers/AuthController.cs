using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.RegisterAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}