using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Waybon.Application.Emails.Abstractions;
using Waybon.Application.Emails.Dtos;

namespace Waybon.Infrastructure.Emails;

public sealed class EmailQueue(ILogger<EmailQueue> logger) : IEmailQueue
{
    // ===================================
    // Constants
    // ===================================

    private const int Capacity = 500;


    // ===================================
    // Channel
    // ===================================

    private readonly Channel<EmailMessage> channel = Channel.CreateBounded<EmailMessage>(new BoundedChannelOptions(Capacity)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = true
    });

    public ChannelReader<EmailMessage> Reader => channel.Reader;


    // ===================================
    // Enqueue
    // ===================================

    public void Enqueue(EmailMessage message)
    {
        if (!channel.Writer.TryWrite(message))
        {
            logger.LogWarning("The email queue is full. An email was dropped.");
        }
    }
}