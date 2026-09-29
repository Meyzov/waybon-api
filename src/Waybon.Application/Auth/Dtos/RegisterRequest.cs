using System.ComponentModel.DataAnnotations;
using Waybon.Domain.Entities;

namespace Waybon.Application.Auth.Dtos;

public sealed class RegisterRequest
{
    [Required]
    [MinLength(User.UsernameMinLength)]
    [MaxLength(User.UsernameMaxLength)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(User.EmailMaxLength)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(UserCredential.PasswordMinLength)]
    [MaxLength(UserCredential.PasswordMaxLength)]
    public string Password { get; set; } = string.Empty;
}