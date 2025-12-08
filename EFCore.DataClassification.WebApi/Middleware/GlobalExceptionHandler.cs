using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EFCore.DataClassification.Exceptions;

namespace EFCore.DataClassification.WebApi.Middleware;

/// <summary>
/// Global exception handler for consistent error responses.
/// Implements .NET 8 IExceptionHandler pattern.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        var problemDetails = CreateProblemDetails(exception, httpContext);

        httpContext.Response.StatusCode = problemDetails.Status ?? 500;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private ProblemDetails CreateProblemDetails(Exception exception, HttpContext context)
    {
        return exception switch
        {
            DataClassificationException dcEx => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Data Classification Error",
                Detail = dcEx.Message,
                Instance = context.Request.Path,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
            },
            
            ArgumentNullException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = _environment.IsDevelopment() ? exception.Message : "Invalid request parameters",
                Instance = context.Request.Path
            },
            
            InvalidOperationException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Operation Failed",
                Detail = _environment.IsDevelopment() ? exception.Message : "The operation could not be completed",
                Instance = context.Request.Path
            },
            
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = _environment.IsDevelopment() ? exception.Message : "An error occurred. Please contact support.",
                Instance = context.Request.Path
            }
        };
    }
}
