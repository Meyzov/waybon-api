using Microsoft.EntityFrameworkCore;
using Waybon.Application.Auth;
using Waybon.Application.Auth.Abstractions;
using Waybon.Application.Auth.Dtos;
using Waybon.Application.Common.Abstractions;
using Waybon.Application.Common.Exceptions;
using Waybon.Application.Emails.Abstractions;
using Waybon.Application.Roles.Abstractions;
using Waybon.Domain.Entities;
using Waybon.Domain.Enums;
using Waybon.Domain.Exceptions;
using Waybon.Infrastructure.Emails.Templates;
using Waybon.Infrastructure.Persistence;

namespace Waybon.Infrastructure.Auth;

public sealed class AuthService(AppDbContext context, IRoleService roleService, IPasswordHasher passwordHasher, ITokenGenerator tokenGenerator, IEmailQueue emailQueue, IEmailSendLimiter emailSendLimiter) : IAuthService
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
    private const string InvalidVerificationTokenMessage = "Invalid or expired verification token.";
    private const string InvalidCodeMessage = "Invalid or expired code.";
    private const string CodeRequestTooSoonMessage = "Please wait before requesting another code.";
    private const string NoDefaultRoleMessage = "No default role is configured.";
    private const string VerificationTokenKey = "verificationToken";

    private static string? dummyPasswordHash;


    // ===================================
    // RegisterAsync
    // ===================================

    public async Task<AuthUserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // ===================================
        // Build user
        // ===================================

        var defaultRole = await roleService.GetDefaultAsync(cancellationToken) ?? throw new InvalidOperationException(NoDefaultRoleMessage);
        var newUser = new User(request.Username, request.Email, defaultRole.Id);


        // ===================================
        // Check existing account
        // ===================================

        var existingUser = await context.Users
            .AsNoTracking()
            .Where(user => user.Email == newUser.Email)
            .Select(user => new { user.Id, user.EmailVerified })
            .FirstOrDefaultAsync(cancellationToken);

        if (existingUser is { EmailVerified: true }) throw new ConflictException(EmailTakenMessage);


        // ===================================
        // Save account
        // ===================================

        var passwordHash = passwordHasher.HashPassword(request.Password);
        var strategy = context.Database.CreateExecutionStrategy();

        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                context.ChangeTracker.Clear();
                await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

                // ===================================
                // Begin Transaction
                // ===================================

                if (existingUser is not null)
                {
                    await context.Users
                        .Where(user => user.Id == existingUser.Id && !user.EmailVerified)
                        .ExecuteDeleteAsync(cancellationToken);
                }

                context.Users.Add(newUser);
                context.UserCredentials.Add(new UserCredential(newUser.Id, passwordHash));
                await context.SaveChangesAsync(cancellationToken);

                // ===================================
                // End Transaction
                // ===================================

                await transaction.CommitAsync(cancellationToken);
            });
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
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
        // ===================================
        // Find account
        // ===================================

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


        // ===================================
        // Check password
        // ===================================

        if (credential.IsLocked()) throw new AccountLockedException(AccountLockedMessage);

        var now = DateTimeOffset.UtcNow;
        DateTimeOffset? lockedUntil = now.Add(UserCredential.LockoutDuration);

        var reserved = await context.UserCredentials
            .Where(existing => existing.Id == credential.Id && (existing.LockedUntil == null || existing.LockedUntil <= now))
            .ExecuteUpdateAsync
            (
                setters => setters
                    .SetProperty(existing => existing.FailedLoginAttempts, existing => (existing.LockedUntil == null ? existing.FailedLoginAttempts : 0) + 1)
                    .SetProperty(existing => existing.LockedUntil, existing => (existing.LockedUntil == null ? existing.FailedLoginAttempts : 0) + 1 >= UserCredential.MaxFailedLoginAttempts ? lockedUntil : null)
                    .SetProperty(existing => existing.UpdatedAt, now),
                cancellationToken
            );

        if (reserved == 0) throw new AccountLockedException(AccountLockedMessage);
        if (!passwordHasher.VerifyPassword(request.Password, credential.PasswordHash)) throw new UnauthorizedException(InvalidCredentialsMessage);

        await context.UserCredentials
            .Where(existing => existing.Id == credential.Id)
            .ExecuteUpdateAsync
            (
                setters => setters
                    .SetProperty(existing => existing.FailedLoginAttempts, 0)
                    .SetProperty(existing => existing.LockedUntil, (DateTimeOffset?)null)
                    .SetProperty(existing => existing.UpdatedAt, now),
                cancellationToken
            );


        // ===================================
        // Check account status
        // ===================================

        if (!user.IsActive) throw new ForbiddenException(AuthErrorCodes.AccountDisabled, AccountDisabledMessage);


        // ===================================
        // Require email verification
        // ===================================

        if (!user.EmailVerified)
        {
            var verificationToken = tokenGenerator.Generate();
            var verificationCode = new VerificationCode(user.Id, VerificationPurpose.EmailVerification, tokenGenerator.Hash(verificationToken));

            await ReplaceInTransactionAsync(async () =>
            {
                await context.VerificationCodes
                    .Where(existing => existing.UserId == user.Id && existing.Purpose == VerificationPurpose.EmailVerification)
                    .ExecuteDeleteAsync(cancellationToken);

                context.VerificationCodes.Add(verificationCode);
            },
            cancellationToken);

            throw new ForbiddenException
            (
                AuthErrorCodes.EmailNotVerified,
                EmailNotVerifiedMessage,
                new Dictionary<string, object?> { [VerificationTokenKey] = verificationToken }
            );
        }


        // ===================================
        // Create session
        // ===================================

        var token = tokenGenerator.Generate();
        var session = new Session(user.Id, tokenGenerator.Hash(token));

        await ReplaceInTransactionAsync(async () =>
        {
            await context.Sessions
                .Where(existing => existing.UserId == user.Id)
                .ExecuteDeleteAsync(cancellationToken);

            context.Sessions.Add(session);
        },
        cancellationToken);

        return new LoginResponse
        {
            Token = token,
            User = ToAuthUserResponse(user)
        };
    }


    // ===================================
    // SendVerificationCodeAsync
    // ===================================

    public async Task SendVerificationCodeAsync(SendVerificationCodeRequest request, CancellationToken cancellationToken = default)
    {
        // ===================================
        // Find verification
        // ===================================

        var tokenHash = tokenGenerator.Hash(request.VerificationToken);

        var pending = await context.VerificationCodes
            .Where(code => code.TokenHash == tokenHash && code.Purpose == VerificationPurpose.EmailVerification)
            .Join
            (
                context.Users,
                code => code.UserId,
                user => user.Id,
                (code, user) => new { Verification = code, user.Email, user.EmailVerified }
            )
            .FirstOrDefaultAsync(cancellationToken);

        if (pending is null || pending.EmailVerified || pending.Verification.IsTokenExpired()) throw new BadRequestException(InvalidVerificationTokenMessage);


        // ===================================
        // Check send limit
        // ===================================

        if (!emailSendLimiter.TryAcquire(pending.Email, out var retryAfter)) throw new TooManyRequestsException(CodeRequestTooSoonMessage, retryAfter);


        // ===================================
        // Send code
        // ===================================

        var code = tokenGenerator.GenerateNumericCode(VerificationCode.CodeLength);
        pending.Verification.SetCode(tokenGenerator.Hash(code));
        await context.SaveChangesAsync(cancellationToken);

        emailQueue.Enqueue(VerificationCodeEmail.Create(pending.Email, code));
    }


    // ===================================
    // VerifyEmailAsync
    // ===================================

    public async Task<LoginResponse> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        // ===================================
        // Find verification
        // ===================================

        var tokenHash = tokenGenerator.Hash(request.VerificationToken);

        var verification = await context.VerificationCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(code => code.TokenHash == tokenHash && code.Purpose == VerificationPurpose.EmailVerification, cancellationToken);

        if (verification is null || verification.IsTokenExpired()) throw new BadRequestException(InvalidVerificationTokenMessage);
        if (!verification.CanAttemptCode()) throw new BadRequestException(InvalidCodeMessage);


        // ===================================
        // Check code
        // ===================================

        var reserved = await context.VerificationCodes
            .Where(code => code.Id == verification.Id && code.FailedAttempts < VerificationCode.MaxFailedAttempts)
            .ExecuteUpdateAsync
            (
                setters => setters.SetProperty(code => code.FailedAttempts, code => code.FailedAttempts + 1),
                cancellationToken
            );

        if (reserved == 0 || !verification.MatchesCode(tokenGenerator.Hash(request.Code))) throw new BadRequestException(InvalidCodeMessage);


        // ===================================
        // Verify user
        // ===================================

        var user = await context.Users.FirstAsync(user => user.Id == verification.UserId, cancellationToken);
        if (!user.IsActive) throw new ForbiddenException(AuthErrorCodes.AccountDisabled, AccountDisabledMessage);

        user.VerifyEmail();


        // ===================================
        // Create session
        // ===================================

        var token = tokenGenerator.Generate();
        var session = new Session(user.Id, tokenGenerator.Hash(token));

        await ReplaceInTransactionAsync(async () =>
        {
            var deleted = await context.VerificationCodes
                .Where(code => code.Id == verification.Id)
                .ExecuteDeleteAsync(cancellationToken);

            if (deleted == 0) throw new BadRequestException(InvalidVerificationTokenMessage);

            await context.Sessions
                .Where(existing => existing.UserId == user.Id)
                .ExecuteDeleteAsync(cancellationToken);

            context.Sessions.Add(session);
        },
        cancellationToken);

        return new LoginResponse
        {
            Token = token,
            User = ToAuthUserResponse(user)
        };
    }


    // ===================================
    // Helpers
    // ===================================

    private async Task ReplaceInTransactionAsync(Func<Task> replace, CancellationToken cancellationToken)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

                // ===================================
                // Begin Transaction
                // ===================================

                await replace();
                await context.SaveChangesAsync(acceptAllChangesOnSuccess: false, cancellationToken);

                // ===================================
                // End Transaction
                // ===================================

                await transaction.CommitAsync(cancellationToken);
            });

            context.ChangeTracker.AcceptAllChanges();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            context.ChangeTracker.Clear();
            throw new ConflictException(LoginConflictMessage);
        }
    }

    private string GetDummyPasswordHash() => dummyPasswordHash ??= passwordHasher.HashPassword(Guid.NewGuid().ToString());

    private static AuthUserResponse ToAuthUserResponse(User user) => new() { Id = user.Id, Username = user.Username, Email = user.Email };
}