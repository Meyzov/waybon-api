using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Waybon.Application.Common.Exceptions;
using Waybon.Application.Emails.Abstractions;
using Waybon.Application.Emails.Dtos;

namespace Waybon.Infrastructure.Emails;

public sealed class BrevoEmailSender(HttpClient httpClient, IOptions<BrevoOptions> options, ILogger<BrevoEmailSender> logger) : IEmailSender
{
    // ===================================
    // Constants
    // ===================================

    private const string SendEmailPath = "smtp/email";
    private const string DeliveryFailedMessage = "The email could not be sent. Try again later.";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };


    // ===================================
    // SendAsync
    // ===================================

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var sender = new BrevoContact(settings.SenderEmail, settings.SenderName);

        var request = new BrevoSendEmailRequest
        (
            Sender: sender,
            To: [new BrevoContact(message.To)],
            ReplyTo: sender,
            Subject: message.Subject,
            HtmlContent: message.HtmlContent,
            TextContent: message.TextContent
        );

        try
        {
            using var response = await httpClient.PostAsJsonAsync(SendEmailPath, request, JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return;

            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError
            (
                "Brevo rejected the email. Status: {StatusCode}. Response: {Response}",
                (int)response.StatusCode,
                error
            );
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Could not reach Brevo.");
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Brevo did not respond in time.");
        }

        throw new EmailDeliveryException(DeliveryFailedMessage);
    }


    // ===================================
    // Brevo contracts
    // ===================================

    private sealed record BrevoContact(string Email, string? Name = null);

    private sealed record BrevoSendEmailRequest
    (
        BrevoContact Sender,
        IReadOnlyList<BrevoContact> To,
        BrevoContact ReplyTo,
        string Subject,
        string HtmlContent,
        string TextContent
    );
}