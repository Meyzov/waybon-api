using Waybon.Application.Common.Abstractions;

namespace Waybon.Infrastructure.Common;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    // ===================================
    // Constants
    // ===================================

    private const int WorkFactor = 12;


    // ===================================
    // HashPassword
    // ===================================

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor: WorkFactor);
    }


    // ===================================
    // VerifyPassword
    // ===================================

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
    }
}