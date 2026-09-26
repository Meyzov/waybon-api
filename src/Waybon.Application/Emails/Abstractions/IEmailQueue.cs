using Waybon.Application.Emails.Dtos;

namespace Waybon.Application.Emails.Abstractions;

public interface IEmailQueue
{
    void Enqueue(EmailMessage message);
}