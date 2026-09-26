using Microsoft.EntityFrameworkCore;
using Npgsql;
using Waybon.Application.Auth;
using Waybon.Application.Auth.Abstractions;
using Waybon.Application.Auth.Dtos;
using Waybon.Application.Common.Abstractions;
using Waybon.Application.Common.Exceptions;
using Waybon.Application.Roles.Abstractions;
using Waybon.Domain.Entities;
using Waybon.Domain.Exceptions;
using Waybon.Infrastructure.Persistence;

namespace Waybon.Infrastructure.Auth;

public sealed class AuthService(AppDbContext context, IRoleService roleService, IPasswordHasher passwordHasher, ITokenGenerator tokenGenerator) : IAuthService
{
    // ===================================
    // Constants
    // ===================================

    private const string EmailTakenMessage = "An account with that email already exists.";
    private const string InvalidCredentialsMessage = "Invalid email or password.";
    private const string AccountLockedMessage = "Account is currently locked.";
    private const string AccountDisabledMessage = "Account is disabled.";
    private const string EmailNotVerifiedMessage = "Email not verified.";
    private const string LoginConflictMessage = "Another login is in progress. Try again.";

    private static string? dummyPasswordHash;


    // ===================================
    // RegisterAsync
    // ===================================

    public async Task<AuthUserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
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

        return ToAuthUserResponse(newUser);
    }


    // ===================================
    // LoginAsync
    // ===================================

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = User.NormalizeEmail(request.Email);

        var account = await context.Users
            .Where(user => user.Email == email)
            .Join
            (
                context.UserCredentials,
                user => user.Id,
                credential => credential.UserId,
                (user, credential) => new { User = user, Credential = credential }
            )
            .FirstOrDefaultAsync(cancellationToken);

        if (account is null)
        {
            passwordHasher.VerifyPassword(request.Password, GetDummyPasswordHash());
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        var user = account.User;
        var credential = account.Credential;

        if (credential.IsLocked())
        {
            throw new AccountLockedException(AccountLockedMessage);
        }

        if (!passwordHasher.VerifyPassword(request.Password, credential.PasswordHash))
        {
            credential.RegisterFailedLogin();
            await context.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        credential.RegisterSuccessfulLogin();

        if (!user.IsActive)
        {
            await context.SaveChangesAsync(cancellationToken);
            throw new ForbiddenException(AuthErrorCodes.AccountDisabled, AccountDisabledMessage);
        }

        if (!user.EmailVerified)
        {
            await context.SaveChangesAsync(cancellationToken);
            throw new ForbiddenException(AuthErrorCodes.EmailNotVerified, EmailNotVerifiedMessage);
        }

        var token = tokenGenerator.Generate();
        var session = new Session(user.Id, tokenGenerator.Hash(token));

        await ReplaceSessionAsync(session, cancellationToken);

        return new LoginResponse
        {
            Token = token,
            User = ToAuthUserResponse(user)
        };
    }


    // ===================================
    // Helpers
    // ===================================

    private async Task ReplaceSessionAsync(Session session, CancellationToken cancellationToken)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

                await context.Sessions
                    .Where(existing => existing.UserId == session.UserId)
                    .ExecuteDeleteAsync(cancellationToken);

                context.Sessions.Add(session);
                await context.SaveChangesAsync(acceptAllChangesOnSuccess: false, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            });

            context.ChangeTracker.AcceptAllChanges();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            context.ChangeTracker.Clear();
            throw new ConflictException(LoginConflictMessage);
        }
    }

    private string GetDummyPasswordHash()
    {
        return dummyPasswordHash ??= passwordHasher.HashPassword(Guid.NewGuid().ToString());
    }

    private static AuthUserResponse ToAuthUserResponse(User user)
    {
        return new AuthUserResponse
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