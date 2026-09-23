using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Waybon.Application.Common.Exceptions;
using Waybon.Domain.Exceptions;

namespace Waybon.Api.Exceptions;

public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, detail) = exception switch
        {
            DomainValidationException ex => (StatusCodes.Status400BadRequest, ex.Message),
            ConflictException ex => (StatusCodes.Status409Conflict, ex.Message),
            AccountLockedException ex => (StatusCodes.Status423Locked, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = ReasonPhrases.GetReasonPhrase(statusCode),
                Detail = detail
            }
        });
    }
}