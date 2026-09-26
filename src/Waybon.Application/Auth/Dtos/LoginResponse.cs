namespace Waybon.Application.Auth.Dtos;

public sealed class LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public AuthUserResponse User { get; init; } = null!;
}