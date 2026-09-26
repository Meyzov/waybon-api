using System.ComponentModel.DataAnnotations;
using Waybon.Domain.Entities;

namespace Waybon.Application.Users.Dtos;

public sealed class UpdateUserRequest
{
    [MinLength(User.UsernameMinLength)]
    [MaxLength(User.UsernameMaxLength)]
    public string? Username { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}