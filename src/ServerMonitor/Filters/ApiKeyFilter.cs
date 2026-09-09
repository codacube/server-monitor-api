// This filter intercepts incoming HTTP requests, verifies the X-API-KEY header
// against appsettings.json, and short-circuits execution with HTTP 401 Unauthorized
// if validation fails

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ServerMonitor.Filters;

public class ApiKeyFilter : IEndpointFilter
{
    private const string HeaderName = "X-API-KEY";
    private readonly IConfiguration _configuration;

    public ApiKeyFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        // 1. Check if the header exists
        if (!httpContext.Request.Headers.TryGetValue(HeaderName, out var extractedKey))
        {
            return Results.Problem(
                detail: "API Key header is missing.", 
                statusCode: StatusCodes.Status401Unauthorized);
        }

        // 2. Fetch expected key from configuration
        var expectedKey = _configuration["ApiKey"];

        // 3. Validate key matching
        if (string.IsNullOrEmpty(expectedKey) || !expectedKey.Equals(extractedKey))
        {
            return Results.Problem(
                detail: "Invalid API Key provided.", 
                statusCode: StatusCodes.Status401Unauthorized);
        }

        // 4. Pass execution to the next filter or route handler
        return await next(context);
    }
}