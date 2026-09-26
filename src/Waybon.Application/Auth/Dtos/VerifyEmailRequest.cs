using System.ComponentModel.DataAnnotations;
using Waybon.Domain.Entities;

namespace Waybon.Application.Auth.Dtos;

public sealed class VerifyEmailRequest
{
    [Required]
    [MaxLength(64)]
    public string VerificationToken { get; set; } = string.Empty;

    [Required]
    [StringLength(VerificationCode.CodeLength, MinimumLength = VerificationCode.CodeLength)]
    [RegularExpression("^[0-9]+$")]
    public string Code { get; set; } = string.Empty;
}