using Waybon.Application.Auth.Dtos;

namespace Waybon.Application.Auth.Abstractions;

public interface IAuthService
{
    Task<AuthUserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}