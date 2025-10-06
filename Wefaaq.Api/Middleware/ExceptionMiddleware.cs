using System.Net;
using System.Text.Json;

namespace Wefaaq.Api.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case KeyNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = exception.Message;
                response.StatusCode = context.Response.StatusCode;
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = exception.Message;
                response.StatusCode = context.Response.StatusCode;
                break;

            case ArgumentException:
            case InvalidOperationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = exception.Message;
                response.StatusCode = context.Response.StatusCode;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = _environment.IsDevelopment()
                    ? exception.Message
                    : "An internal server error occurred. Please try again later.";
                response.StatusCode = context.Response.StatusCode;

                if (_environment.IsDevelopment())
                {
                    response.StackTrace = exception.StackTrace;
                    response.InnerException = exception.InnerException?.Message;
                }
                break;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Error response model
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }
}

/// <summary>
/// Extension method for adding the exception middleware to the pipeline
/// </summary>
public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}
