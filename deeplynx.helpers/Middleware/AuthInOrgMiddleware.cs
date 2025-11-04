using deeplynx.helpers.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace deeplynx.helpers;

// Attribute to decorate controllers/actions
// Example usage: [AuthInOrg("read", "organization")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AuthInOrgAttribute : Attribute
{
    public AuthInOrgAttribute(string action, string resource)
    {
        Action = action;
        Resource = resource;
    }

    public string Action { get; set; } 
    public string Resource { get; set; } 
}

public class AuthInOrgMiddleware
{
    private readonly RequestDelegate _next;

    public AuthInOrgMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IOrgRolePermissionService rolePermissionService)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }
        
        

        // Get all AuthInOrg attributes from the endpoint
        var authAttributes = endpoint.Metadata
            .GetOrderedMetadata<AuthInOrgAttribute>();
        
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
        int orgId = 0;
        var routeOrgId = context.GetRouteValue("orgId")?.ToString();
        if (!string.IsNullOrEmpty(routeOrgId) && int.TryParse(routeOrgId, out orgId))
        {
            // Found in route
        }
        // 2. Then try query parameters
        else if (context.Request.Query.TryGetValue("orgId", out var queryOrgId) 
                 && int.TryParse(queryOrgId.FirstOrDefault(), out orgId))
        {
            // Found in query
        }
        // 3. no org for you 
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "Bad Request: Invalid or missing orgId" });
            return;
        }

        // Check each permission requirement
        foreach (var authAttr in authAttributes)
        {
            // Check if user's roles in this project have the required permission
            var hasPermission = await rolePermissionService.PermissionInOrg(
                userId,
                orgId,
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
                        orgId,
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