namespace Waybon.Application.Common.Exceptions;

public sealed class TooManyRequestsException(string message, TimeSpan retryAfter) : Exception(message)
{
    public TimeSpan RetryAfter { get; } = retryAfter;
}