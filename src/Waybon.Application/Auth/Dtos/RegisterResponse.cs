namespace Waybon.Application.Auth.Dtos;

public sealed class RegisterResponse
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}