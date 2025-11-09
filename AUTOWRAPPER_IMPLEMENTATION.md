# AutoWrapper Response Wrapping Implementation Guide

## Overview
AutoWrapper has been successfully implemented in the Wefaaq API project to provide standardized API response wrapping for all endpoints.

## What Was Done

### 1. Package Installation
- **Package**: `AutoWrapper.Core` (v4.5.1)
- **Location**: Wefaaq.Api project
- **Installation Command**: `dotnet add package AutoWrapper.Core`

### 2. Configuration Class Created
- **File**: `Wefaaq.Api\Extensions\AutoWrapperExtensions.cs`
- **Purpose**: Centralized configuration for AutoWrapper middleware
- **Key Features**:
  - API version display (configurable, default: "1.0")
  - Status code in responses
  - IsError flag for successful responses
  - Response and exception logging enabled
  - Request data logging on exceptions
  - Reference loop handling for JSON serialization
  - HTML validation bypass
  - Only wraps routes starting with "/api"

### 3. Middleware Registration
- **File**: `Wefaaq.Api\Program.cs`
- **Location**: After authentication/authorization, before endpoint mapping
- **Usage**: `app.UseAutoWrapperConfig(apiVersion: "1.0");`

### 4. Exception Handling
- **Change**: Commented out custom `ExceptionMiddleware` in Program.cs
- **Reason**: AutoWrapper provides built-in exception handling
- **Note**: The custom middleware is preserved but disabled

## Response Format

### Success Response (200 OK)
```json
{
  "version": "1.0",
  "statusCode": 200,
  "message": "GET Request successful.",
  "isError": false,
  "result": {
    // Your actual data here
  }
}
```

### Error Response (400 Bad Request)
```json
{
  "version": "1.0",
  "statusCode": 400,
  "message": "Validation failed.",
  "isError": true,
  "errors": [
    // Validation errors if any
  ]
}
```

### Error Response (404 Not Found)
```json
{
  "version": "1.0",
  "statusCode": 404,
  "message": "Resource not found.",
  "isError": true
}
```

### Error Response (500 Internal Server Error)
```json
{
  "version": "1.0",
  "statusCode": 500,
  "message": "An error occurred while processing your request.",
  "isError": true
}
```

## Controller Usage

Controllers can continue using standard ASP.NET Core return methods without manual wrapping:

```csharp
// Success responses
return Ok(data);                    // Auto-wrapped as 200 with result
return CreatedAtAction(...);        // Auto-wrapped as 201 with result
return NoContent();                 // Auto-wrapped as 204

// Error responses
return NotFound(new { message = "Not found" });      // Auto-wrapped as 404
return BadRequest(ModelState);                       // Auto-wrapped as 400
```

## Configuration Options

The `UseAutoWrapperConfig()` extension method supports the following configuration:

```csharp
// Default configuration (API version 1.0)
app.UseAutoWrapperConfig();

// Custom API version
app.UseAutoWrapperConfig(apiVersion: "2.0");
```

## Middleware Pipeline Order

The correct order in Program.cs:
1. Exception Handler (commented out - AutoWrapper handles exceptions)
2. Swagger (development only)
3. HTTPS Redirection
4. CORS
5. Authentication (if added)
6. Authorization
7. **AutoWrapper** ? Added here
8. MapControllers

## Advanced Configuration

To customize AutoWrapper options, modify the `AutoWrapperExtensions.cs` file:

```csharp
var options = new AutoWrapperOptions
{
    // Show/hide API version
    ShowApiVersion = true,
    ApiVersion = apiVersion,
    
    // Show/hide status code
    ShowStatusCode = true,
    
    // Show isError flag for successful responses
    ShowIsErrorFlagForSuccessfulResponse = true,
    
    // Enable logging
    EnableResponseLogging = true,
    EnableExceptionLogging = true,
    LogRequestDataOnException = true,
    
    // JSON configuration
    IgnoreNullValue = false,
    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
    
    // Validation
    BypassHTMLValidation = true,
    
    // API-only mode (only wrap API endpoints)
    IsApiOnly = true,
    WrapWhenApiPathStartsWith = "/api"
};
```

## Testing

To test the implementation:

1. **Start the API**: Run the project
2. **Test GET endpoint**: 
   - URL: `GET /api/clients/get-all`
   - Expected: Response wrapped with version, statusCode, message, isError, and result
3. **Test POST endpoint**:
   - URL: `POST /api/clients`
   - Expected: 201 response wrapped with created data
4. **Test validation errors**:
   - URL: `POST /api/clients` with invalid data
   - Expected: 400 response wrapped with validation errors
5. **Test not found**:
   - URL: `GET /api/clients/{non-existent-guid}`
   - Expected: 404 response wrapped with error message

## Important Notes

1. **Exception Handling**: AutoWrapper automatically catches and wraps exceptions. The custom `ExceptionMiddleware` has been disabled but preserved for reference.

2. **JSON Serialization**: AutoWrapper uses Newtonsoft.Json for serialization. Your existing System.Text.Json configuration in controllers still works, but AutoWrapper's response wrapping uses Newtonsoft.Json.

3. **Reference Loop Handling**: Configured to ignore reference loops in both AutoWrapper and System.Text.Json.

4. **Swagger Integration**: AutoWrapper works seamlessly with Swagger. Response examples in Swagger will show the wrapped format.

5. **API-Only Mode**: Currently configured to only wrap responses for routes starting with "/api". Other routes (like Swagger UI) are not wrapped.

## Troubleshooting

### If responses are not being wrapped:
1. Ensure AutoWrapper is placed BEFORE `app.MapControllers()`
2. Check that your endpoints start with "/api"
3. Verify the package is properly installed

### If getting serialization errors:
1. Check for circular references in your DTOs
2. Verify ReferenceLoopHandling is set to Ignore
3. Ensure DTOs are properly configured with navigation properties

### If exception details are not showing:
1. Check `EnableExceptionLogging` is true
2. Verify `LogRequestDataOnException` is enabled
3. Review logs for exception details

## Re-enabling Custom Exception Middleware (If Needed)

If you need custom exception handling alongside AutoWrapper:
1. Uncomment `app.UseGlobalExceptionHandler();` in Program.cs
2. Place it BEFORE AutoWrapper
3. Modify ExceptionMiddleware to throw exceptions instead of writing responses
4. Let AutoWrapper catch and wrap the exceptions

## Files Modified/Created

### Created:
- `Wefaaq.Api\Extensions\AutoWrapperExtensions.cs` - AutoWrapper configuration

### Modified:
- `Wefaaq.Api\Program.cs` - Added AutoWrapper middleware, commented out custom exception handler
- `Wefaaq.Api\Wefaaq.Api.csproj` - Added AutoWrapper.Core package reference

## Next Steps

1. Test all API endpoints to ensure proper wrapping
2. Update API documentation to reflect the new response format
3. Update frontend/client code to handle the wrapped responses
4. Consider adding custom error messages for different exception types
5. Configure custom response messages if needed

## References

- [AutoWrapper GitHub](https://github.com/proudmonkey/AutoWrapper)
- [AutoWrapper Documentation](https://github.com/proudmonkey/AutoWrapper/wiki)
