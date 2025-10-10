using System.Security.Claims;
using deeplynx.datalayer.Models;
using deeplynx.helpers.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace deeplynx.helpers;

public class UserContextMiddleware
{
    private readonly ILogger<UserContextMiddleware> _logger;
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public UserContextMiddleware(
        RequestDelegate next,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<UserContextMiddleware> logger)
    {
        _next = next;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("User is authenticated, extracting user context");

                // Log all claims for debugging
                var allClaims = context.User.Claims.Select(c => $"{c.Type}={c.Value}");
                _logger.LogInformation($"Available claims: {string.Join(", ", allClaims)}");

                // Try to extract email from multiple possible claim types
                var email = ExtractEmail(context.User);

                if (!string.IsNullOrEmpty(email))
                {
                    _logger.LogInformation($"Email extracted: {email}");
                    UserContextStorage.Email = email;

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DeeplynxContext>();
                        var user = await dbContext.Users
                            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                        if (user != null)
                        {
                            UserContextStorage.UserId = user.Id;
                            _logger.LogInformation($"User found: {user.Email} (ID: {user.Id})");
                        }
                        else
                        {
                            _logger.LogWarning($"User with email {email} not found in database");
                            UserContextStorage.UserId = 0;
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Could not extract email from claims");
                    UserContextStorage.Email = null;
                    UserContextStorage.UserId = 0;
                }
            }
            else
            {
                _logger.LogInformation("User is not authenticated");
                UserContextStorage.Email = null;
                UserContextStorage.UserId = 0;
            }

            await _next(context);
        }
        finally
        {
            // Always clear after request completes
            UserContextStorage.Email = null;
            UserContextStorage.UserId = 0;
        }
    }

    private string? ExtractEmail(ClaimsPrincipal user)
    {
        // Try various claim types in order of preference

        // 1. Standard email claim
        var email = user.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(email)) return email;

        // 2. Simple "email" claim (common in JWT)
        email = user.FindFirst("email")?.Value;
        if (!string.IsNullOrEmpty(email)) return email;

        // 3. Subject claim (Okta uses this for email sometimes)
        email = user.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(email)) return email;

        // 4. Name claim (your HS256 token uses this)
        email = user.FindFirst(ClaimTypes.Name)?.Value;
        if (!string.IsNullOrEmpty(email)) return email;

        // 5. Simple "name" claim
        email = user.FindFirst("name")?.Value;
        if (!string.IsNullOrEmpty(email)) return email;

        // 6. NameIdentifier with namespace (Okta format)
        email = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        if (!string.IsNullOrEmpty(email)) return email;

        // 7. Preferred username
        email = user.FindFirst("preferred_username")?.Value;
        if (!string.IsNullOrEmpty(email)) return email;

        // 8. Username
        email = user.FindFirst("username")?.Value;
        if (!string.IsNullOrEmpty(email)) return email;

        return null;
    }
}