using deeplynx.helpers.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace deeplynx.helpers;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next, ILogger<UserContextMiddleware> logger)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Debug: Log all available claims
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Try multiple common claim names for username including namespaced claims
                UserContextStorage.Username = context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            }
            else
            {
                UserContextStorage.Username = null;
            }

            await _next(context);
        }
        finally
        {
            // Always clear after request completes
            UserContextStorage.Username = null;
        }
    }
}
