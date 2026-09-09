using Microsoft.EntityFrameworkCore;
using Npgsql;
using Waybon.Application.Common.Exceptions;
using Waybon.Application.Roles.Abstractions;
using Waybon.Application.Roles.Dtos;
using Waybon.Domain.Entities;
using Waybon.Infrastructure.Persistence;

namespace Waybon.Infrastructure.Roles;

public sealed class RoleService(AppDbContext context) : IRoleService
{
    public async Task<IReadOnlyList<RoleResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Roles
            .AsNoTracking()
            .Select(role => new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<RoleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Roles
            .AsNoTracking()
            .Select(role => new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            })
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<RoleResponse> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var newRole = new Role(request.Name);
        context.Roles.Add(newRole);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new ConflictException("The role already exists.");
        }

        return new RoleResponse
        {
            Id = newRole.Id,
            Name = newRole.Name,
            CreatedAt = newRole.CreatedAt,
            UpdatedAt = newRole.UpdatedAt
        };
    }

    public async Task<RoleResponse?> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await context.Roles.FindAsync([id], cancellationToken);
        if (role is null) return null;

        role.UpdateName(request.Name);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new ConflictException("The role already exists.");
        }

        return new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await context.Roles.FindAsync([id], cancellationToken);
        if (role is null) return false;

        context.Roles.Remove(role);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
        {
            throw new ConflictException
            (
                "The role cannot be deleted because it is assigned to one or more users."
            );
        }

        return true;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        };
    }

    private static bool IsForeignKeyViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation,
        };
    }
}