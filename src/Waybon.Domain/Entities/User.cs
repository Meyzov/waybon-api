using System.Net.Mail;
using Waybon.Domain.Exceptions;

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
        Id = Guid.CreateVersion7();
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
        var normalizedUsername = NormalizeUsername(newUsername);
        if (normalizedUsername == Username) return;

        Username = normalizedUsername;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new DomainValidationException
            (
                "Username is required."
            );
        }

        var normalizedName = username.Trim();

        if (normalizedName.Any(char.IsWhiteSpace))
        {
            throw new DomainValidationException
            (
                "Username cannot contain spaces."
            );
        }

        if (normalizedName.Length < 3)
        {
            throw new DomainValidationException
            (
                "Username must be at least 3 characters long."
            );
        }

        if (normalizedName.Length > 30)
        {
            throw new DomainValidationException
            (
                "Username cannot exceed 30 characters."
            );
        }

        return normalizedName;
    }

    public void UpdateEmail(string newEmail)
    {
        var normalizedEmail = NormalizeEmail(newEmail);
        if (normalizedEmail == Email) return;

        Email = normalizedEmail;
        EmailVerified = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainValidationException
            (
                "Email is required."
            );
        }

        var normalizedEmail = email.Trim();

        try
        {
            var mailAddress = new MailAddress(normalizedEmail);

            if (mailAddress.Address != normalizedEmail)
            {
                throw new DomainValidationException
                (
                    "Invalid email format."
                );
            }

            return normalizedEmail.ToLowerInvariant();
        }
        catch (FormatException)
        {
            throw new DomainValidationException
            (
                "Invalid email format."
            );
        }
    }

    public void UpdateRoleId(Guid newRoleId)
    {
        var validatedRoleId = ValidateRoleId(newRoleId);
        if (validatedRoleId == RoleId) return;

        RoleId = validatedRoleId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static Guid ValidateRoleId(Guid roleId)
    {
        if (roleId == Guid.Empty)
        {
            throw new DomainValidationException
            (
                "Role ID cannot be empty."
            );
        }

        return roleId;
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void VerifyEmail()
    {
        if (EmailVerified) return;

        EmailVerified = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}