using System.Security.Claims;
using deeplynx.helpers.Context;
using deeplynx.interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace deeplynx.helpers.Middleware;

// Attribute to decorate controllers/actions
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AuthInProjectAttribute : Attribute
{
    public AuthInProjectAttribute(string action, string resource)
    {
        Action = action;
        Resource = resource;
    }

    public string Action { get; set; } // e.g., "read", "write"
    public string Resource { get; set; } // e.g., "project", "record", "class"
}

public class RoleBasedAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public RoleBasedAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IRolePermissionService rolePermissionService)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        // Get all AuthInProject attributes from the endpoint
        var authAttributes = endpoint.Metadata
            .GetOrderedMetadata<AuthInProjectAttribute>();

        // If no auth attributes, return
        if (!authAttributes.Any())
        {
            await _next(context);
            return;
        }
        
        var userId = UserContextStorage.UserId;

        if (userId <= 0)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
            return;
        }

        // Get projectId from route
        //TODO: Update routes that are passing projectId as a query param instead of in the route or add logic for param projectId
        var projectIdString = context.GetRouteValue("projectId")?.ToString();
        if (string.IsNullOrEmpty(projectIdString) || !int.TryParse(projectIdString, out var projectId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "Bad Request: Invalid or missing projectId" });
            return;
        }

        // Check each permission requirement
        foreach (var authAttr in authAttributes)
        {
            // Check if user's roles in this project have the required permission
            var hasPermission = await rolePermissionService.UserHasPermissionInProjectAsync(
                userId,
                projectId,
                authAttr.Action,
                authAttr.Resource
            );

            if (!hasPermission)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Forbidden: User role does not have required permissions",
                    required = new
                    {
                        projectId,
                        action = authAttr.Action,
                        resource = authAttr.Resource
                    }
                });
                return;
            }
        }

        // All permission checks passed
        await _next(context);
    }
}