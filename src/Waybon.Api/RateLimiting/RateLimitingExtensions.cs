using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;

namespace Waybon.Api.RateLimiting;

public static class RateLimitingExtensions
{
    // ===================================
    // Constants
    // ===================================

    private const int RegisterPermitLimit = 5;
    private const int LoginPermitLimit = 10;
    private const int VerificationPermitLimit = 10;
    private static readonly TimeSpan RegisterWindow = TimeSpan.FromHours(1);
    private static readonly TimeSpan LoginWindow = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan VerificationWindow = TimeSpan.FromMinutes(1);

    private const string UnknownClient = "unknown";
    private const string TooManyRequestsMessage = "Too many requests. Try again later.";


    // ===================================
    // AddAuthRateLimiting
    // ===================================

    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services)
    {
        return services.AddRateLimiter(options =>
        {
            options.AddPolicy(RateLimitPolicies.Register, httpContext => FixedWindowByIp(httpContext, RegisterPermitLimit, RegisterWindow));
            options.AddPolicy(RateLimitPolicies.Login, httpContext => FixedWindowByIp(httpContext, LoginPermitLimit, LoginWindow));
            options.AddPolicy(RateLimitPolicies.Verification, httpContext => FixedWindowByIp(httpContext, VerificationPermitLimit, VerificationWindow));
            options.OnRejected = WriteRejectionAsync;
        });
    }


    // ===================================
    // Helpers
    // ===================================

    private static RateLimitPartition<string> FixedWindowByIp(HttpContext httpContext, int permitLimit, TimeSpan window)
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? UnknownClient;

        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window,
            QueueLimit = 0
        });
    }

    private static async ValueTask WriteRejectionAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;
        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)) httpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();

        var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status429TooManyRequests),
                Detail = TooManyRequestsMessage
            }
        });
    }
}