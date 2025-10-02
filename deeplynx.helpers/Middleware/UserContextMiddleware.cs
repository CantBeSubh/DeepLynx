using deeplynx.datalayer.Models;
using deeplynx.helpers.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.helpers;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public UserContextMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
    {
        _next = next;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Debug: Log all available claims
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Try multiple common claim names for username including namespaced claims
                UserContextStorage.Email = context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                if (!string.IsNullOrEmpty(UserContextStorage.Email))
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DeeplynxContext>();
                        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == UserContextStorage.Email.ToLower());
                        if (user != null)
                        {
                            UserContextStorage.UserId = user.Id;
                        }
                    }
                }
            }
            else
            {
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
}
