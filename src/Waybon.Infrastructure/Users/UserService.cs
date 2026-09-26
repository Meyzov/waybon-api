using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Waybon.Application.Common.Exceptions;
using Waybon.Application.Users.Abstractions;
using Waybon.Application.Users.Dtos;
using Waybon.Domain.Entities;
using Waybon.Infrastructure.Persistence;

namespace Waybon.Infrastructure.Users;

public sealed class UserService(AppDbContext context) : IUserService
{
    // ===================================
    // Mapping
    // ===================================

    private static readonly Expression<Func<User, UserResponse>> ToResponseProjection = user => new UserResponse
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        RoleId = user.RoleId,
        IsActive = user.IsActive,
        EmailVerified = user.EmailVerified,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };

    private static readonly Func<User, UserResponse> ToResponse = ToResponseProjection.Compile();


    // ===================================
    // GetAllAsync
    // ===================================

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AsNoTracking()
            .OrderBy(user => user.Id)
            .Select(ToResponseProjection)
            .ToListAsync(cancellationToken);
    }


    // ===================================
    // GetByIdAsync
    // ===================================

    public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AsNoTracking()
            .Where(user => user.Id == id)
            .Select(ToResponseProjection)
            .FirstOrDefaultAsync(cancellationToken);
    }


    // ===================================
    // UpdateAsync
    // ===================================

    public async Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FindAsync([id], cancellationToken);
        if (user is null) return null;

        if (request.Username is not null)
        {
            user.UpdateUsername(request.Username);
        }

        if (request.Email is not null)
        {
            user.UpdateEmail(request.Email);
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            context.ChangeTracker.Clear();
            throw new ConflictException("A user with that email already exists.");
        }

        return ToResponse(user);
    }


    // ===================================
    // DeleteAsync
    // ===================================

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FindAsync([id], cancellationToken);
        if (user is null) return false;

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);

        return true;
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
}