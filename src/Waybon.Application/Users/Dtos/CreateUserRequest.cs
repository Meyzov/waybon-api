using System.ComponentModel.DataAnnotations;
using Waybon.Domain.Entities;

namespace Waybon.Application.Users.Dtos;

public sealed class CreateUserRequest
{
    [Required]
    [MinLength(User.UsernameMinLength)]
    [MaxLength(User.UsernameMaxLength)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}