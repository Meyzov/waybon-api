using System.Linq.Expressions;
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
    // ===================================
    // Mapping
    // ===================================

    private static readonly Expression<Func<Role, RoleResponse>> ToResponseProjection = role => new RoleResponse
    {
        Id = role.Id,
        Name = role.Name,
        IsDefault = role.IsDefault,
        CreatedAt = role.CreatedAt,
        UpdatedAt = role.UpdatedAt
    };

    private static readonly Func<Role, RoleResponse> ToResponse = ToResponseProjection.Compile();


    // ===================================
    // GetAllAsync
    // ===================================

    public async Task<IReadOnlyList<RoleResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Roles
            .AsNoTracking()
            .OrderBy(role => role.Id)
            .Select(ToResponseProjection)
            .ToListAsync(cancellationToken);
    }


    // ===================================
    // GetByIdAsync
    // ===================================

    public async Task<RoleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Roles
            .AsNoTracking()
            .Where(role => role.Id == id)
            .Select(ToResponseProjection)
            .FirstOrDefaultAsync(cancellationToken);
    }


    // ===================================
    // GetByNameAsync
    // ===================================

    public async Task<RoleResponse?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        var normalizedName = name.Trim().ToLowerInvariant();
        return await context.Roles
            .AsNoTracking()
            .Where(role => role.Name == normalizedName)
            .Select(ToResponseProjection)
            .FirstOrDefaultAsync(cancellationToken);
    }


    // ===================================
    // CreateAsync
    // ===================================

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
            context.ChangeTracker.Clear();
            throw new ConflictException("The role already exists.");
        }

        return ToResponse(newRole);
    }


    // ===================================
    // UpdateAsync
    // ===================================

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
            context.ChangeTracker.Clear();
            throw new ConflictException("The role already exists.");
        }

        return ToResponse(role);
    }


    // ===================================
    // DeleteAsync
    // ===================================

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await context.Roles.FindAsync([id], cancellationToken);
        if (role is null) return false;

        if (role.IsDefault)
        {
            throw new ConflictException("The default role cannot be deleted. Set another role as default first.");
        }

        context.Roles.Remove(role);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
        {
            context.ChangeTracker.Clear();
            throw new ConflictException
            (
                "The role cannot be deleted because it is assigned to one or more users."
            );
        }

        return true;
    }


    // ===================================
    // GetDefaultAsync
    // ===================================

    public async Task<RoleResponse?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await context.Roles
            .AsNoTracking()
            .Where(role => role.IsDefault)
            .Select(ToResponseProjection)
            .FirstOrDefaultAsync(cancellationToken);
    }


    // ===================================
    // SetDefaultAsync
    // ===================================

    public async Task<RoleResponse?> SetDefaultAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        try
        {
            return await strategy.ExecuteAsync(async () =>
            {
                context.ChangeTracker.Clear();

                var role = await context.Roles.FindAsync([id], cancellationToken);
                if (role is null) return null;

                if (!role.IsDefault)
                {
                    await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

                    var currentDefault = await context.Roles.FirstOrDefaultAsync(r => r.IsDefault, cancellationToken);
                    if (currentDefault is not null)
                    {
                        currentDefault.UnmarkAsDefault();
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    role.MarkAsDefault();
                    await context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);
                }

                return ToResponse(role);
            });
        }
        catch (DbUpdateException ex) when (IsDefaultRoleViolation(ex))
        {
            context.ChangeTracker.Clear();
            throw new ConflictException("The default role was changed by another request. Try again.");
        }
    }


    // ===================================
    // Helpers
    // ===================================

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
            SqlState: PostgresErrorCodes.ForeignKeyViolation
        };
    }

    private static bool IsDefaultRoleViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "ix_role_is_default"
        };
    }
}