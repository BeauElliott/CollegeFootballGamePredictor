namespace Api.Middleware;

/// <summary>
/// Request logging middleware to log all incoming HTTP requests and their processing time.
/// Helps with debugging and monitoring API usage.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var requestId = Guid.NewGuid().ToString();
        
        _logger.LogInformation(
            "Request [{RequestId}] {Method} {Path} started at {StartTime}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            startTime);

        try
        {
            await _next(context);
        }
        finally
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Request [{RequestId}] {Method} {Path} completed with status {StatusCode} in {Duration}ms",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                duration.TotalMilliseconds);
        }
    }
}
