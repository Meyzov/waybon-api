using System.ComponentModel.DataAnnotations;

namespace Waybon.Application.Roles.Dtos;

public sealed class CreateRoleRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(25)]
    public string Name { get; set; } = string.Empty;
}