using System.Security.Cryptography;
using System.Text;
using Waybon.Domain.Enums;
using Waybon.Domain.Exceptions;

namespace Waybon.Domain.Entities;

public sealed class VerificationCode
{
    // ===================================
    // Constants
    // ===================================

    public const int HashLength = 64;
    public const int PurposeMaxLength = 30;
    public const int CodeLength = 6;
    public const int MaxFailedAttempts = 5;
    public static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);
    public static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(15);


    // ===================================
    // Constructors
    // ===================================

    private VerificationCode()
    {

    }

    public VerificationCode(Guid userId, VerificationPurpose purpose, string tokenHash)
    {
        Id = Guid.CreateVersion7();
        UserId = ValidateUserId(userId);
        Purpose = ValidatePurpose(purpose);
        TokenHash = ValidateHash(tokenHash);
        FailedAttempts = 0;
        CreatedAt = DateTimeOffset.UtcNow;
        TokenExpiresAt = CreatedAt.Add(TokenLifetime);
    }


    // ===================================
    // Properties
    // ===================================

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public VerificationPurpose Purpose { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTimeOffset TokenExpiresAt { get; private set; }
    public string? CodeHash { get; private set; }
    public DateTimeOffset? CodeExpiresAt { get; private set; }
    public int FailedAttempts { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }


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

    private static VerificationPurpose ValidatePurpose(VerificationPurpose purpose)
    {
        if (!Enum.IsDefined(purpose))
        {
            throw new DomainValidationException
            (
                "Verification purpose is not valid."
            );
        }

        return purpose;
    }

    private static string ValidateHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash) || hash.Length != HashLength)
        {
            throw new DomainValidationException
            (
                $"Hash must be exactly {HashLength} characters long."
            );
        }

        return hash;
    }


    // ===================================
    // Token
    // ===================================

    public bool IsTokenExpired()
    {
        return DateTimeOffset.UtcNow >= TokenExpiresAt;
    }


    // ===================================
    // Code
    // ===================================

    public void SetCode(string codeHash)
    {
        var now = DateTimeOffset.UtcNow;

        CodeHash = ValidateHash(codeHash);
        CodeExpiresAt = now.Add(CodeLifetime);
        FailedAttempts = 0;

        if (TokenExpiresAt < CodeExpiresAt)
        {
            TokenExpiresAt = CodeExpiresAt.Value;
        }
    }

    public bool CanAttemptCode()
    {
        return CodeHash is not null
            && CodeExpiresAt > DateTimeOffset.UtcNow
            && FailedAttempts < MaxFailedAttempts;
    }

    public bool MatchesCode(string codeHash)
    {
        if (CodeHash is null) return false;

        return CryptographicOperations.FixedTimeEquals
        (
            Encoding.ASCII.GetBytes(CodeHash),
            Encoding.ASCII.GetBytes(codeHash)
        );
    }

    public void RegisterFailedAttempt()
    {
        FailedAttempts++;
    }
}