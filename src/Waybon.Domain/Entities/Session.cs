using Waybon.Domain.Exceptions;

namespace Waybon.Domain.Entities;

public sealed class Session
{
    // ===================================
    // Constants
    // ===================================

    public const int TokenHashLength = 64;


    // ===================================
    // Constructors
    // ===================================

    private Session()
    {

    }

    public Session(Guid userId, string tokenHash)
    {
        Id = Guid.CreateVersion7();
        UserId = ValidateUserId(userId);
        TokenHash = ValidateTokenHash(tokenHash);
        CreatedAt = DateTimeOffset.UtcNow;
        LastAppActivityAt = CreatedAt;
    }


    // ===================================
    // Properties
    // ===================================

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastAppActivityAt { get; private set; }


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

    private static string ValidateTokenHash(string tokenHash)
    {
        if (string.IsNullOrWhiteSpace(tokenHash) || tokenHash.Length != TokenHashLength)
        {
            throw new DomainValidationException
            (
                $"Token hash must be exactly {TokenHashLength} characters long."
            );
        }

        return tokenHash;
    }
}