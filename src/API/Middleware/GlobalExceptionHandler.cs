using Microsoft.AspNetCore.Diagnostics;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandler(IWebHostEnvironment environment)
    {
        _environment = environment;
    }
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        httpContext.Response.ContentType = "application/json";

        var (statusCode, detailedMessage) = exception switch
        {
            QuoteNotFoundException ex => (StatusCodes.Status400BadRequest, ex.Message),
            InvalidQuoteTokenException ex => (StatusCodes.Status400BadRequest, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        string clientMessage = statusCode == StatusCodes.Status500InternalServerError
        ? detailedMessage
        : "An unexpected error occurred.";

        if (_environment.IsDevelopment())
        {
            clientMessage = detailedMessage;
        }

        httpContext.Response.StatusCode = statusCode;

        var response = new
        {
            status = statusCode,
            message = clientMessage,
        };

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}