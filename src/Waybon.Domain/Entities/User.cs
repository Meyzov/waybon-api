using System.Net.Mail;
using Waybon.Domain.Exceptions;

namespace Waybon.Domain.Entities;

public sealed class User
{
    // ===================================
    // Constants
    // ===================================

    public const int UsernameMinLength = 3;
    public const int UsernameMaxLength = 20;
    public const int EmailMaxLength = 255;


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
    // Username
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

        var normalizedUsername = username.Trim();

        if (normalizedUsername.Length < UsernameMinLength)
        {
            throw new DomainValidationException
            (
                $"Username must be at least {UsernameMinLength} characters long."
            );
        }

        if (normalizedUsername.Length > UsernameMaxLength)
        {
            throw new DomainValidationException
            (
                $"Username cannot exceed {UsernameMaxLength} characters."
            );
        }

        if (!normalizedUsername.All(IsAllowedUsernameCharacter))
        {
            throw new DomainValidationException
            (
                "Username can only contain letters, numbers, dots, hyphens and underscores."
            );
        }

        return normalizedUsername;
    }

    private static bool IsAllowedUsernameCharacter(char character)
    {
        return char.IsAsciiLetterOrDigit(character) || character is '.' or '-' or '_';
    }


    // ===================================
    // Email
    // ===================================

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

        if (normalizedEmail.Length > EmailMaxLength)
        {
            throw new DomainValidationException
            (
                $"Email cannot exceed {EmailMaxLength} characters."
            );
        }

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

    public void VerifyEmail()
    {
        if (EmailVerified) return;

        EmailVerified = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }


    // ===================================
    // Role
    // ===================================

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


    // ===================================
    // Status
    // ===================================

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
}