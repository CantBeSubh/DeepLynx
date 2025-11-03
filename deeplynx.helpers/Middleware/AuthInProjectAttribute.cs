using System.Security.Claims;
using deeplynx.helpers.Context;
using deeplynx.interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace deeplynx.helpers;

// Attribute to decorate controllers/actions
// Example usage: [AuthInProject("read", "project")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AuthInProjectAttribute : Attribute
{
    public AuthInProjectAttribute(string action, string resource)
    {
        Action = action;
        Resource = resource;
    }

    public string Action { get; set; } 
    public string Resource { get; set; } 
}

public class AuthInProjectMiddleware
{
    private readonly RequestDelegate _next;

    public AuthInProjectMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IProjectRolePermissionService rolePermissionService)
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
        
        // 1. First try route values
        int projectId = 0;
        var routeProjectId = context.GetRouteValue("projectId")?.ToString();
        if (!string.IsNullOrEmpty(routeProjectId) && int.TryParse(routeProjectId, out projectId))
        {
            // Found in route
        }
        // 2. Then try query parameters
        else if (context.Request.Query.TryGetValue("projectId", out var queryProjectId) 
                 && int.TryParse(queryProjectId.FirstOrDefault(), out projectId))
        {
            // Found in query
        }
        // 3. no project for you 
        else
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