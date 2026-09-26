using Waybon.Application.Auth.Dtos;

namespace Waybon.Application.Auth.Abstractions;

public interface IAuthService
{
    Task<AuthUserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task SendVerificationCodeAsync(SendVerificationCodeRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default);
}