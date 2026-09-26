using Waybon.Application.Emails.Dtos;

namespace Waybon.Application.Emails.Abstractions;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}