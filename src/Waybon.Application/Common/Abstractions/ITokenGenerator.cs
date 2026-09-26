namespace Waybon.Application.Common.Abstractions;

public interface ITokenGenerator
{
    string Generate();
    string Hash(string token);
    string GenerateNumericCode(int length);
}