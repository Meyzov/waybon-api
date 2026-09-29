using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Waybon.Application.Common.Exceptions;
using Waybon.Application.Emails.Abstractions;

namespace Waybon.Infrastructure.Emails;

public sealed class EmailQueueProcessor(EmailQueue queue, IServiceScopeFactory scopeFactory, ILogger<EmailQueueProcessor> logger) : BackgroundService
{
    // ===================================
    // Constants
    // ===================================

    private const string UnexpectedErrorLogMessage = "Unexpected error while sending a queued email.";


    // ===================================
    // ExecuteAsync
    // ===================================

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                await emailSender.SendAsync(message, stoppingToken);
            }
            catch (EmailDeliveryException)
            {
                // Already logged by the email sender.
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, UnexpectedErrorLogMessage);
            }
        }
    }
}