using System.ComponentModel.DataAnnotations;

namespace Waybon.Application.Users.Dtos;

public sealed class UpdateUserRequest
{
    [MinLength(3)]
    [MaxLength(30)]
    public string? Username { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}