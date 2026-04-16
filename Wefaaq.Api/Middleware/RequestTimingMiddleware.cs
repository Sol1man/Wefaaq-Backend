using System.Diagnostics;

namespace Wefaaq.Api.Middleware;

/// <summary>
/// Logs total wall-clock duration of every HTTP request.
/// Used together with EF Core command logging to diagnose where time is spent
/// (network round-trips vs. controller logic vs. SQL execution).
/// </summary>
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip noise: swagger UI, static assets, health checks
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var ms = sw.ElapsedMilliseconds;
            var status = context.Response.StatusCode;

            // Color-code so slow requests are easy to spot in the console
            var marker = ms switch
            {
                < 200 => "OK ",
                < 800 => "MED",
                _ => "SLOW",
            };

            _logger.LogInformation(
                "[REQ {Marker}] {Method} {Path} -> {Status} in {Elapsed} ms",
                marker, context.Request.Method, path, status, ms);
        }
    }
}

public static class RequestTimingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder app)
        => app.UseMiddleware<RequestTimingMiddleware>();
}
