namespace Waybon.Application.Common.Exceptions;

public sealed class ForbiddenException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}