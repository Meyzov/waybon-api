using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Waybon.Application.Users.Abstractions;
using Waybon.Application.Users.Dtos;

namespace Waybon.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    // ===================================
    // GET: api/v1/users
    // ===================================

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }


    // ===================================
    // GET: api/v1/users/{id}
    // ===================================

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        if (user is null) return Problem
        (
            statusCode: StatusCodes.Status404NotFound,
            detail: "User not found."
        );

        return Ok(user);
    }


    // ===================================
    // PATCH: api/v1/users/{id}
    // ===================================

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<UserResponse>> Update(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await userService.UpdateAsync(id, request, cancellationToken);
        if (user is null) return Problem
        (
            statusCode: StatusCodes.Status404NotFound,
            detail: "User not found."
        );

        return Ok(user);
    }


    // ===================================
    // DELETE: api/v1/users/{id}
    // ===================================

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await userService.DeleteAsync(id, cancellationToken);
        if (!deleted) return Problem
        (
            statusCode: StatusCodes.Status404NotFound,
            detail: "User not found."
        );

        return NoContent();
    }
}