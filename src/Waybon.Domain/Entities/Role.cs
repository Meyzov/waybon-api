using Waybon.Domain.Exceptions;

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
        Id = Guid.CreateVersion7();
        Name = NormalizeName(name);
        IsDefault = false;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }


    // ===================================
    // Properties
    // ===================================

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsDefault { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }


    // ===================================
    // Methods
    // ===================================

    public void UpdateName(string newName)
    {
        var normalizedName = NormalizeName(newName);
        if (normalizedName == Name) return;

        Name = normalizedName;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException
            (
                "Role name is required."
            );
        }

        var normalizedName = name.Trim();
        if (normalizedName.Length < 3)
        {
            throw new DomainValidationException
            (
                "Role name must be at least 3 characters long."
            );
        }

        if (normalizedName.Length > 25)
        {
            throw new DomainValidationException
            (
                "Role name cannot exceed 25 characters."
            );
        }

        return normalizedName.ToLowerInvariant();
    }

    public void MarkAsDefault()
    {
        if (IsDefault) return;

        IsDefault = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UnmarkAsDefault()
    {
        if (!IsDefault) return;

        IsDefault = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}