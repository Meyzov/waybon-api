using Microsoft.AspNetCore.Mvc;
using Waybon.Application.Roles.Abstractions;
using Waybon.Application.Roles.Dtos;

namespace Waybon.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RolesController(IRoleService roleService) : ControllerBase
{
    // ===================================
    // GET: api/roles
    // ===================================

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoleResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var roles = await roleService.GetAllAsync(cancellationToken);
        return Ok(roles);
    }


    // ===================================
    // GET: api/roles/{id}
    // ===================================

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoleResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var role = await roleService.GetByIdAsync(id, cancellationToken);
        if (role is null) return NotFound
        (
            new { error = "Role not found." }
        );

        return Ok(role);
    }


    // ===================================
    // POST: api/roles
    // ===================================

    [HttpPost]
    public async Task<ActionResult<RoleResponse>> Create(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await roleService.CreateAsync(request, cancellationToken);
        return CreatedAtAction
        (
            nameof(GetById),
            new
            {
                id = role.Id
            },
            role
        );
    }


    // ===================================
    // PUT: api/roles/{id}
    // ===================================

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RoleResponse>> Update(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await roleService.UpdateAsync(id, request, cancellationToken);
        if (role is null) return NotFound
        (
            new { error = "Role not found." }
        );

        return Ok(role);
    }


    // ===================================
    // DELETE: api/roles/{id}
    // ===================================

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await roleService.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound
        (
            new { error = "Role not found." }
        );

        return NoContent();
    }
}