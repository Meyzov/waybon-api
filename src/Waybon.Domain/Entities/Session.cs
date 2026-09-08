namespace Waybon.Domain.Entities;

public sealed class Session
{
    // ===================================
    // Constructors
    // ===================================

    private Session()
    {

    }

    public Session(Guid userId, string tokenHash)
    {
        Id = Guid.NewGuid();
        UserId = ValidateUserId(userId);
        TokenHash = ValidateTokenHash(tokenHash);
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }


    // ===================================
    // Properties
    // ===================================

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }


    // ===================================
    // Methods
    // ===================================

    private static Guid ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException
            (
                "User ID is required.", nameof(userId)
            );
        }

        return userId;
    }

    public void UpdateTokenHash(string newTokenHash)
    {
        TokenHash = ValidateTokenHash(newTokenHash);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string ValidateTokenHash(string tokenHash)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException
            (
                "Token hash is required.", nameof(tokenHash)
            );
        }

        return tokenHash;
    }
}