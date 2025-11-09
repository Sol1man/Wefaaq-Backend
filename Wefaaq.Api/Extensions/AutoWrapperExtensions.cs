using AutoWrapper;

namespace Wefaaq.Api.Extensions;

/// <summary>
/// Extension methods for configuring AutoWrapper middleware
/// </summary>
public static class AutoWrapperExtensions
{
    /// <summary>
    /// Configures AutoWrapper middleware with standardized response wrapping
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="apiVersion">API version to display in responses (default: "1.0")</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseAutoWrapperConfig(this IApplicationBuilder app, string apiVersion = "1.0")
    {
        var options = new AutoWrapperOptions
        {
            // API version configuration
            ApiVersion = apiVersion,
            ShowApiVersion = true,
            
            // Status code and error flag configuration
            ShowStatusCode = true,
            ShowIsErrorFlagForSuccessfulResponse = true,
            
            // Logging configuration
            EnableResponseLogging = true,
            EnableExceptionLogging = true,
            LogRequestDataOnException = true,
            
            // JSON serialization configuration
            IgnoreNullValue = false,
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
            
            // Validation configuration
            BypassHTMLValidation = true,
            
            // Message configuration (optional - customize as needed)
            IsApiOnly = true,
            WrapWhenApiPathStartsWith = "/api"
        };

        return app.UseApiResponseAndExceptionWrapper(options);
    }
}
