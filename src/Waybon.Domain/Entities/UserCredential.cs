using Waybon.Domain.Exceptions;

namespace Waybon.Domain.Entities;

public sealed class UserCredential
{
    // ===================================
    // Constructors
    // ===================================

    private UserCredential()
    {

    }

    public UserCredential(Guid userId, string passwordHash)
    {
        Id = Guid.NewGuid();
        UserId = ValidateUserId(userId);
        PasswordHash = ValidatePasswordHash(passwordHash);
        FailedLoginAttempts = 0;
        LockedUntil = null;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
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


    // ===================================
    // Methods
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

        return passwordHash;
    }

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

        if (FailedLoginAttempts >= 5)
        {
            LockedUntil = now.AddMinutes(15);
        }

        UpdatedAt = now;
    }

    public void RegisterSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}