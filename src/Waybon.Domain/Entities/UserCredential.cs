using Waybon.Domain.Exceptions;

namespace Waybon.Domain.Entities;

public sealed class UserCredential
{
    // ===================================
    // Constants
    // ===================================

    public const int PasswordHashMaxLength = 128;
    public const int MaxFailedLoginAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);


    // ===================================
    // Constructors
    // ===================================

    private UserCredential()
    {

    }

    public UserCredential(Guid userId, string passwordHash)
    {
        Id = Guid.CreateVersion7();
        UserId = ValidateUserId(userId);
        PasswordHash = ValidatePasswordHash(passwordHash);
        FailedLoginAttempts = 0;
        LockedUntil = null;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
        PasswordChangedAt = CreatedAt;
    }


    // ===================================
    // Properties
    // ===================================

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string PasswordHash { get; private set; } = null!;
    public int FailedLoginAttempts { get; private set; }
    public DateTimeOffset? LockedUntil { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset PasswordChangedAt { get; private set; }


    // ===================================
    // Validation
    // ===================================

    private static Guid ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainValidationException
            (
                "User ID is required."
            );
        }

        return userId;
    }

    private static string ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainValidationException
            (
                "Password hash is required."
            );
        }

        if (passwordHash.Length > PasswordHashMaxLength)
        {
            throw new DomainValidationException
            (
                $"Password hash cannot exceed {PasswordHashMaxLength} characters."
            );
        }

        return passwordHash;
    }


    // ===================================
    // Login attempts
    // ===================================

    public void RegisterFailedLogin()
    {
        var now = DateTimeOffset.UtcNow;

        if (LockedUntil.HasValue)
        {
            if (now >= LockedUntil.Value)
            {
                FailedLoginAttempts = 0;
                LockedUntil = null;
            }
            else
            {
                throw new AccountLockedException
                (
                    "Account is currently locked."
                );
            }
        }

        FailedLoginAttempts++;

        if (FailedLoginAttempts >= MaxFailedLoginAttempts)
        {
            LockedUntil = now.Add(LockoutDuration);
        }

        UpdatedAt = now;
    }

    public void RegisterSuccessfulLogin()
    {
        if (LockedUntil > DateTimeOffset.UtcNow)
        {
            throw new AccountLockedException
            (
                "Account is currently locked."
            );
        }

        FailedLoginAttempts = 0;
        LockedUntil = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}