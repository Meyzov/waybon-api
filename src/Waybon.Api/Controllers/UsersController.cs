using Microsoft.AspNetCore.Mvc;
using Waybon.Application.Users.Abstractions;
using Waybon.Application.Users.Dtos;

namespace Waybon.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    // ===================================
    // GET: api/users
    // ===================================

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }


    // ===================================
    // GET: api/users/{id}
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
    // POST: api/users
    // ===================================

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction
        (
            nameof(GetById),
            new
            {
                id = user.Id
            },
            user
        );
    }


    // ===================================
    // PATCH: api/users/{id}
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
    // DELETE: api/users/{id}
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