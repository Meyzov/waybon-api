using Waybon.Domain.Exceptions;

namespace Waybon.Domain.Entities;

public sealed class Role
{
    // ===================================
    // Constants
    // ===================================

    public const int NameMinLength = 3;
    public const int NameMaxLength = 15;

    private const string NameRequiredMessage = "Role name is required.";
    private static readonly string NameTooShortMessage = $"Role name must be at least {NameMinLength} characters long.";
    private static readonly string NameTooLongMessage = $"Role name cannot exceed {NameMaxLength} characters.";


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
        if (string.IsNullOrWhiteSpace(name)) throw new DomainValidationException(NameRequiredMessage);

        var normalizedName = name.Trim();

        if (normalizedName.Length < NameMinLength) throw new DomainValidationException(NameTooShortMessage);
        if (normalizedName.Length > NameMaxLength) throw new DomainValidationException(NameTooLongMessage);

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