namespace Waybon.Application.Emails.Abstractions;

public interface IEmailSendLimiter
{
    bool TryAcquire(string email, out TimeSpan retryAfter);
}