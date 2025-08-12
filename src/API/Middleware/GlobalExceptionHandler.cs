using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        LogException(httpContext, exception);

        httpContext.Response.ContentType = "application/json";

        var (statusCode, detailedMessage) = exception switch
        {
            InvalidOperationException ex => (StatusCodes.Status400BadRequest, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, exception.Message),
        };

        string clientMessage =
            statusCode == StatusCodes.Status400BadRequest
                ? detailedMessage
                : "An unexpected error occurred.";

        httpContext.Response.StatusCode = statusCode;

        var response = new ProblemDetails { Status = statusCode, Title = clientMessage };

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }

    private void LogException(HttpContext context, Exception exception)
    {
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        var exceptionType = exception.GetType().Name;
        var exceptionMessage = exception.Message;

        if (context.Response.StatusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Error occurred processing {RequestMethod} {RequestPath}: {ExceptionType}: {ExceptionMessage}",
                requestMethod,
                requestPath,
                exceptionType,
                exceptionMessage
            );
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Warning occurred processing {RequestMethod} {RequestPath}: {ExceptionType}: {ExceptionMessage}",
                requestMethod,
                requestPath,
                exceptionType,
                exceptionMessage
            );
        }
    }
}
