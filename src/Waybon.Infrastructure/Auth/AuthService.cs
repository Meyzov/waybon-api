using Microsoft.EntityFrameworkCore;
using Npgsql;
using Waybon.Application.Auth.Abstractions;
using Waybon.Application.Auth.Dtos;
using Waybon.Application.Common.Abstractions;
using Waybon.Application.Common.Exceptions;
using Waybon.Application.Roles.Abstractions;
using Waybon.Domain.Entities;
using Waybon.Infrastructure.Persistence;

namespace Waybon.Infrastructure.Auth;

public sealed class AuthService(AppDbContext context, IRoleService roleService, IPasswordHasher passwordHasher) : IAuthService
{
    // ===================================
    // Constants
    // ===================================

    private const string EmailTakenMessage = "An account with that email already exists.";


    // ===================================
    // RegisterAsync
    // ===================================

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var defaultRole = await roleService.GetDefaultAsync(cancellationToken) ?? throw new InvalidOperationException("No default role is configured.");
        var newUser = new User(request.Username, request.Email, defaultRole.Id);

        var existingUser = await context.Users
            .AsNoTracking()
            .Where(user => user.Email == newUser.Email)
            .Select(user => new { user.Id, user.EmailVerified })
            .FirstOrDefaultAsync(cancellationToken);

        if (existingUser is { EmailVerified: true })
        {
            throw new ConflictException(EmailTakenMessage);
        }

        var passwordHash = passwordHasher.HashPassword(request.Password);
        var strategy = context.Database.CreateExecutionStrategy();

        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                context.ChangeTracker.Clear();
                await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

                if (existingUser is not null)
                {
                    await context.Users
                        .Where(user => user.Id == existingUser.Id && !user.EmailVerified)
                        .ExecuteDeleteAsync(cancellationToken);
                }

                context.Users.Add(newUser);
                context.UserCredentials.Add(new UserCredential(newUser.Id, passwordHash));
                await context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            });
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            context.ChangeTracker.Clear();
            throw new ConflictException(EmailTakenMessage);
        }

        return ToResponse(newUser);
    }


    // ===================================
    // Helpers
    // ===================================

    private static RegisterResponse ToResponse(User user)
    {
        return new RegisterResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        };
    }
}