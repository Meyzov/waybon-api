using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Waybon.Application.Common.Exceptions;
using Waybon.Domain.Exceptions;

namespace Waybon.Api.Exceptions;

public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    // ===================================
    // TryHandleAsync
    // ===================================

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            httpContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            return true;
        }

        var (statusCode, detail) = exception switch
        {
            DomainValidationException ex => (StatusCodes.Status400BadRequest, ex.Message),
            BadRequestException ex => (StatusCodes.Status400BadRequest, ex.Message),
            UnauthorizedException ex => (StatusCodes.Status401Unauthorized, ex.Message),
            ForbiddenException ex => (StatusCodes.Status403Forbidden, ex.Message),
            ConflictException ex => (StatusCodes.Status409Conflict, ex.Message),
            AccountLockedException ex => (StatusCodes.Status423Locked, ex.Message),
            TooManyRequestsException ex => (StatusCodes.Status429TooManyRequests, ex.Message),
            EmailDeliveryException ex => (StatusCodes.Status503ServiceUnavailable, ex.Message),

            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError
            (
                exception,
                "Unhandled exception on {Method} {Path}. TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                Activity.Current?.Id ?? httpContext.TraceIdentifier
            );
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = detail
        };

        if (exception is ForbiddenException forbiddenException)
        {
            problemDetails.Extensions["code"] = forbiddenException.Code;

            foreach (var (key, value) in forbiddenException.Extensions)
            {
                problemDetails.Extensions[key] = value;
            }
        }

        if (exception is TooManyRequestsException tooManyRequestsException)
        {
            var seconds = (int)Math.Ceiling(tooManyRequestsException.RetryAfter.TotalSeconds);
            httpContext.Response.Headers.RetryAfter = seconds.ToString();
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }
}