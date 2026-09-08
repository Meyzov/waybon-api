namespace Waybon.Domain.Entities;

public sealed class Role
{
    // ===================================
    // Constructors
    // ===================================

    private Role()
    {

    }

    public Role(string name)
    {
        Id = Guid.NewGuid();
        Name = NormalizeName(name);
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }


    // ===================================
    // Properties
    // ===================================

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }


    // ===================================
    // Methods
    // ===================================

    public void UpdateName(string newName)
    {
        Name = NormalizeName(newName);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException
            (
                "Role name is required.", nameof(name)
            );
        }

        var normalizedName = name.Trim();
        if (normalizedName.Length > 25)
        {
            throw new ArgumentException
            (
                "Role name cannot exceed 25 characters.", nameof(name)
            );
        }

        return normalizedName.ToLowerInvariant();
    }
}