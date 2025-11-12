using deeplynx.helpers.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace deeplynx.helpers;

// Attribute to decorate controllers/actions
// Example usage: 
// Multiple Separate Attributes
// [Auth("read", "document")]
// [Auth("write", "document")]
// [Auth("delete", "document")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AuthAttribute : Attribute
{
    public AuthAttribute(string action, string resource)
    {
        Action = action;
        Resource = resource;
    }

    public string Action { get; set; } 
    public string Resource { get; set; } 
}

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IOrgRolePermissionService orgRolePermissionService, IProjectRolePermissionService projectRolePermissionService, ISysAdminService sysAdminService)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        // Get all AuthInOrg attributes from the endpoint
        var authAttributes = endpoint.Metadata
            .GetOrderedMetadata<AuthAttribute>();
        
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

        bool isSysAdmin = await sysAdminService.SysAdminCheck(userId);

        if (!isSysAdmin)
        {
            // Extract both organizationId and projectId from route or query
            int orgId = 0;
            int projectId = 0;

            // Try to get organizationId
            var routeOrgId = context.GetRouteValue("organizationId")?.ToString();
            if (!string.IsNullOrEmpty(routeOrgId) && int.TryParse(routeOrgId, out orgId))
            {
                // Found organizationId in route
            }
            else if (context.Request.Query.TryGetValue("organizationId", out var queryOrgId)
                     && int.TryParse(queryOrgId.FirstOrDefault(), out orgId))
            {
                // Found organizationId in query
            }

            // Try to get projectId
            var routeProjectId = context.GetRouteValue("projectId")?.ToString();
            if (!string.IsNullOrEmpty(routeProjectId) && int.TryParse(routeProjectId, out projectId))
            {
                // Found projectId in route
            }
            else if (context.Request.Query.TryGetValue("projectId", out var queryProjectId)
                     && int.TryParse(queryProjectId.FirstOrDefault(), out projectId))
            {
                // Found projectId in query
            }

            // Must have at least one of organizationId or projectId
            if (orgId <= 0 && projectId <= 0)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                    { error = "Bad Request: Invalid or missing organization or project ID value" });
                return;
            }

            // Check each permission requirement
            foreach (var authAttr in authAttributes)
            {
                bool hasOrgPermission = false;
                bool hasProjectPermission = false;

                // Check organization permission if orgId is present
                if (orgId > 0)
                {
                    hasOrgPermission = await orgRolePermissionService.PermissionInOrg(
                        userId,
                        orgId,
                        authAttr.Action,
                        authAttr.Resource
                    );
                }

                // Check project permission if projectId is present
                if (projectId > 0)
                {
                    hasProjectPermission = await projectRolePermissionService.PermissionInProject(
                        userId,
                        projectId,
                        authAttr.Action,
                        authAttr.Resource
                    );
                }

                // User needs permission in at least one scope (org OR project)
                // Only fail if they have neither org permission nor project permission
                bool hasPermission = hasOrgPermission || hasProjectPermission;

                if (!hasPermission)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Forbidden: User role does not have required permissions in organization or project",
                        required = new
                        {
                            orgId = orgId > 0 ? orgId : (int?)null,
                            projectId = projectId > 0 ? projectId : (int?)null,
                            action = authAttr.Action,
                            resource = authAttr.Resource
                        }
                    });
                    return;
                }
            }
        }

        // All permission checks passed
        await _next(context);
    }
}