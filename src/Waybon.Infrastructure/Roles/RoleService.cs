using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Waybon.Application.Common.Exceptions;
using Waybon.Application.Roles.Abstractions;
using Waybon.Application.Roles.Dtos;
using Waybon.Domain.Entities;
using Waybon.Infrastructure.Persistence;

namespace Waybon.Infrastructure.Roles;

public sealed class RoleService(AppDbContext context) : IRoleService
{
    // ===================================
    // Constants
    // ===================================

    private const string RoleExistsMessage = "The role already exists.";
    private const string DefaultRoleDeleteMessage = "The default role cannot be deleted. Set another role as default first.";
    private const string RoleInUseMessage = "The role cannot be deleted because it is assigned to one or more users.";
    private const string DefaultRoleConflictMessage = "The default role was changed by another request. Try again.";
    private const string DefaultRoleIndexName = "ix_role_is_default";


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
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            context.ChangeTracker.Clear();
            throw new ConflictException(RoleExistsMessage);
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
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            context.ChangeTracker.Clear();
            throw new ConflictException(RoleExistsMessage);
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
        if (role.IsDefault) throw new ConflictException(DefaultRoleDeleteMessage);

        context.Roles.Remove(role);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.IsForeignKeyViolation())
        {
            context.ChangeTracker.Clear();
            throw new ConflictException(RoleInUseMessage);
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

                    // ===================================
                    // Begin Transaction
                    // ===================================

                    var currentDefault = await context.Roles.FirstOrDefaultAsync(existing => existing.IsDefault, cancellationToken);
                    if (currentDefault is not null)
                    {
                        currentDefault.UnmarkAsDefault();
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    role.MarkAsDefault();
                    await context.SaveChangesAsync(cancellationToken);

                    // ===================================
                    // End Transaction
                    // ===================================

                    await transaction.CommitAsync(cancellationToken);
                }

                return ToResponse(role);
            });
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation(DefaultRoleIndexName))
        {
            context.ChangeTracker.Clear();
            throw new ConflictException(DefaultRoleConflictMessage);
        }
    }
}