using System.ComponentModel.DataAnnotations;

namespace Waybon.Application.Users.Dtos;

public sealed class CreateUserRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(30)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(72)]
    public string Password { get; set; } = string.Empty;
}