using System.ComponentModel.DataAnnotations;
using Waybon.Domain.Entities;

namespace Waybon.Application.Roles.Dtos;

public sealed class UpdateRoleRequest
{
    [Required]
    [MinLength(Role.NameMinLength)]
    [MaxLength(Role.NameMaxLength)]
    public string Name { get; set; } = string.Empty;
}