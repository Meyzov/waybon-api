using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Waybon.Application.Common.Abstractions;

namespace Waybon.Infrastructure.Common;

public sealed class SecureTokenGenerator : ITokenGenerator
{
    // ===================================
    // Constants
    // ===================================

    private const int TokenSizeInBytes = 32;


    // ===================================
    // Generate
    // ===================================

    public string Generate()
    {
        Span<byte> bytes = stackalloc byte[TokenSizeInBytes];
        RandomNumberGenerator.Fill(bytes);

        return Base64Url.EncodeToString(bytes);
    }


    // ===================================
    // Hash
    // ===================================

    public string Hash(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexStringLower(hash);
    }
}