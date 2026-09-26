using Waybon.Application.Auth.Dtos;

namespace Waybon.Application.Auth.Abstractions;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}