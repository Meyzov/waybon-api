using Microsoft.AspNetCore.Diagnostics;
using Waybon.Application.Common.Exceptions;
using Waybon.Domain.Exceptions;

namespace Waybon.Api.Exceptions;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var response = exception switch
        {
            DomainValidationException ex => new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ex.Message
            },
            
            ConflictException ex => new
            {
                StatusCode = StatusCodes.Status409Conflict,
                ex.Message
            },

            AccountLockedException ex => new
            {
                StatusCode = StatusCodes.Status423Locked,
                ex.Message
            },

            _ => new
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "An unexpected error occurred."
            }
        };

        httpContext.Response.StatusCode = response.StatusCode;
        await httpContext.Response.WriteAsJsonAsync
        (
            new { error = response.Message }, cancellationToken
        );

        return true;
    }
}