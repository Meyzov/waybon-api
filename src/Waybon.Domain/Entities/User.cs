using System.Net.Mail;

namespace Waybon.Domain.Entities;

public sealed class User
{
    // ===================================
    // Constructors
    // ===================================

    private User()
    {

    }

    public User(string username, string email, Guid roleId)
    {
        Id = Guid.NewGuid();
        Username = NormalizeUsername(username);
        Email = NormalizeEmail(email);
        RoleId = ValidateRoleId(roleId);
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }


    // ===================================
    // Properties
    // ===================================

    public Guid Id { get; private set; }
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public Guid RoleId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool EmailVerified { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }


    // ===================================
    // Methods
    // ===================================

    public void UpdateUsername(string newUsername)
    {
        Username = NormalizeUsername(newUsername);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException
            (
                "Username is required.", nameof(username)
            );
        }

        var normalizedName = username.Trim();

        if (normalizedName.Any(char.IsWhiteSpace))
        {
            throw new ArgumentException
            (
                "Username cannot contain spaces.", nameof(username)
            );
        }

        if (normalizedName.Length < 3)
        {
            throw new ArgumentException
            (
                "Username must be at least 3 characters long.", nameof(username)
            );
        }

        if (normalizedName.Length > 30)
        {
            throw new ArgumentException
            (
                "Username cannot exceed 30 characters.", nameof(username)
            );
        }

        return normalizedName;
    }

    public void UpdateEmail(string newEmail)
    {
        Email = NormalizeEmail(newEmail);
        EmailVerified = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException
            (
                "Email is required.", nameof(email)
            );
        }

        var normalizedEmail = email.Trim();

        try
        {
            var mailAddress = new MailAddress(normalizedEmail);

            if (mailAddress.Address != normalizedEmail)
            {
                throw new ArgumentException
                (
                    "Invalid email format.", nameof(email)
                );
            }

            return normalizedEmail.ToLowerInvariant();
        }
        catch (FormatException)
        {
            throw new ArgumentException
            (
                "Invalid email format.", nameof(email)
            );
        }
    }

    public void UpdateRoleId(Guid newRoleId)
    {
        RoleId = ValidateRoleId(newRoleId);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static Guid ValidateRoleId(Guid roleId)
    {
        if (roleId == Guid.Empty)
        {
            throw new ArgumentException
            (
                "Role ID cannot be empty.", nameof(roleId)
            );
        }

        return roleId;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void VerifyEmail()
    {
        EmailVerified = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}