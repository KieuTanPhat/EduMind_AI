using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Api.Infrastructure;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            ValidationException validationException =>
                (StatusCodes.Status400BadRequest, "Validation failed.", string.Join(" ", validationException.Errors.Select(x => x.ErrorMessage))),
            BadRequestException => (StatusCodes.Status400BadRequest, "Bad request.", exception.Message),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict.", exception.Message),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized.", exception.Message),
            NotFoundException => (StatusCodes.Status404NotFound, "Not found.", exception.Message),
            ExternalServiceException => (StatusCodes.Status503ServiceUnavailable, "Request unavailable.", "Tạm thời không thể xử lý yêu cầu. Vui lòng thử lại sau."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", "The server could not complete the request.")
        };

        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception while processing {RequestPath}", httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Request failed with status code {StatusCode}", statusCode);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode >= 500 && _environment.IsDevelopment() ? exception.Message : detail,
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
