namespace Waybon.Application.Emails.Dtos;

public sealed record EmailMessage
(
    string To,
    string Subject,
    string HtmlContent,
    string TextContent
);