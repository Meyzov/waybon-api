using Waybon.Domain.Exceptions;

namespace Waybon.Domain.Entities;

public sealed class Role
{
    // ===================================
    // Constants
    // ===================================

    public const int NameMinLength = 3;
    public const int NameMaxLength = 15;


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
    // Name
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

        if (normalizedName.Length < NameMinLength)
        {
            throw new DomainValidationException
            (
                $"Role name must be at least {NameMinLength} characters long."
            );
        }

        if (normalizedName.Length > NameMaxLength)
        {
            throw new DomainValidationException
            (
                $"Role name cannot exceed {NameMaxLength} characters."
            );
        }

        return normalizedName.ToLowerInvariant();
    }


    // ===================================
    // Default role
    // ===================================

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