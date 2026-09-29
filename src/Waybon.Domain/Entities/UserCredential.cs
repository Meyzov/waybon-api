using Waybon.Domain.Exceptions;

namespace Waybon.Domain.Entities;

public sealed class UserCredential
{
    // ===================================
    // Constants
    // ===================================

    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 128;
    public const int PasswordHashMaxLength = 128;
    public const int MaxFailedLoginAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private const string UserIdRequiredMessage = "User ID is required.";
    private const string PasswordHashRequiredMessage = "Password hash is required.";
    private static readonly string PasswordHashTooLongMessage = $"Password hash cannot exceed {PasswordHashMaxLength} characters.";


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
        if (userId == Guid.Empty) throw new DomainValidationException(UserIdRequiredMessage);
        return userId;
    }

    private static string ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new DomainValidationException(PasswordHashRequiredMessage);
        if (passwordHash.Length > PasswordHashMaxLength) throw new DomainValidationException(PasswordHashTooLongMessage);

        return passwordHash;
    }


    // ===================================
    // Login attempts
    // ===================================

    public bool IsLocked() => LockedUntil > DateTimeOffset.UtcNow;


    // ===================================
    // Password
    // ===================================

    public void ChangePassword(string newPasswordHash)
    {
        var now = DateTimeOffset.UtcNow;

        PasswordHash = ValidatePasswordHash(newPasswordHash);
        FailedLoginAttempts = 0;
        LockedUntil = null;
        PasswordChangedAt = now;
        UpdatedAt = now;
    }
}