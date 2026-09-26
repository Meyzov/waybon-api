namespace Waybon.Application.Common.Exceptions;

public sealed class ForbiddenException(string code, string message, IReadOnlyDictionary<string, object?>? extensions = null) : Exception(message)
{
    public string Code { get; } = code;
    public IReadOnlyDictionary<string, object?> Extensions { get; } = extensions ?? new Dictionary<string, object?>();
}