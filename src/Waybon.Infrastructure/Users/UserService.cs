using Microsoft.EntityFrameworkCore;
using Npgsql;
using Waybon.Application.Common.Abstractions;
using Waybon.Application.Common.Exceptions;
using Waybon.Application.Roles.Abstractions;
using Waybon.Application.Users.Abstractions;
using Waybon.Application.Users.Dtos;
using Waybon.Domain.Entities;
using Waybon.Infrastructure.Persistence;

namespace Waybon.Infrastructure.Users;

public sealed class UserService(AppDbContext context, IRoleService roleService, IPasswordHasher passwordHasher) : IUserService
{
    // ===================================
    // GetAllAsync
    // ===================================

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AsNoTracking()
            .OrderBy(user => user.Id)
            .Select(user => new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RoleId = user.RoleId,
                IsActive = user.IsActive,
                EmailVerified = user.EmailVerified,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }


    // ===================================
    // GetByIdAsync
    // ===================================

    public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AsNoTracking()
            .Select(user => new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RoleId = user.RoleId,
                IsActive = user.IsActive,
                EmailVerified = user.EmailVerified,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            })
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }


    // ===================================
    // CreateAsync
    // ===================================

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var defaultRole = await roleService.GetDefaultAsync(cancellationToken) ?? throw new InvalidOperationException("No default role is configured.");

        var newUser = new User(request.Username, request.Email, defaultRole.Id);
        var passwordHash = passwordHasher.HashPassword(request.Password);
        var newCredential = new UserCredential(newUser.Id, passwordHash);

        context.Users.Add(newUser);
        context.UserCredentials.Add(newCredential);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            context.ChangeTracker.Clear();
            throw new ConflictException("A user with that email already exists.");
        }

        return new UserResponse
        {
            Id = newUser.Id,
            Username = newUser.Username,
            Email = newUser.Email,
            RoleId = newUser.RoleId,
            IsActive = newUser.IsActive,
            EmailVerified = newUser.EmailVerified,
            CreatedAt = newUser.CreatedAt,
            UpdatedAt = newUser.UpdatedAt
        };
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

        return new UserResponse
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