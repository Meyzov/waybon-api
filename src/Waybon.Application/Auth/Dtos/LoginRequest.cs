using System.ComponentModel.DataAnnotations;
using Waybon.Domain.Entities;

namespace Waybon.Application.Auth.Dtos;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(User.EmailMaxLength)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}