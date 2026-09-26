using Microsoft.Extensions.Caching.Memory;
using Waybon.Application.Emails.Abstractions;

namespace Waybon.Infrastructure.Emails;

public sealed class MemoryEmailSendLimiter(IMemoryCache cache) : IEmailSendLimiter
{
    // ===================================
    // Constants
    // ===================================

    private const int MaxEmailsPerWindow = 5;
    private static readonly TimeSpan Window = TimeSpan.FromHours(24);
    private static readonly TimeSpan Cooldown = TimeSpan.FromSeconds(60);

    private readonly Lock gate = new();


    // ===================================
    // TryAcquire
    // ===================================

    public bool TryAcquire(string email, out TimeSpan retryAfter)
    {
        var now = DateTimeOffset.UtcNow;
        var key = $"email-send:{email}";

        lock (gate)
        {
            if (!cache.TryGetValue(key, out SendWindow? window) || window is null)
            {
                window = new SendWindow(now);
                cache.Set(key, window, window.StartedAt.Add(Window));
            }

            var sinceLastSend = now - window.LastSentAt;
            if (sinceLastSend < Cooldown)
            {
                retryAfter = Cooldown - sinceLastSend;
                return false;
            }

            if (window.Count >= MaxEmailsPerWindow)
            {
                retryAfter = window.StartedAt.Add(Window) - now;
                return false;
            }

            window.Count++;
            window.LastSentAt = now;

            retryAfter = TimeSpan.Zero;
            return true;
        }
    }


    // ===================================
    // Send window
    // ===================================

    private sealed class SendWindow(DateTimeOffset startedAt)
    {
        public DateTimeOffset StartedAt { get; } = startedAt;
        public DateTimeOffset LastSentAt { get; set; } = DateTimeOffset.MinValue;
        public int Count { get; set; }
    }
}