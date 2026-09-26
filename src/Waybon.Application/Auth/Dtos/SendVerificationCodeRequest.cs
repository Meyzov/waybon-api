using System.ComponentModel.DataAnnotations;

namespace Waybon.Application.Auth.Dtos;

public sealed class SendVerificationCodeRequest
{
    [Required]
    [MaxLength(64)]
    public string VerificationToken { get; set; } = string.Empty;
}