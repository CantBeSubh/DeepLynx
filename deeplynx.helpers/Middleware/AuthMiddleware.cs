using deeplynx.helpers.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

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

   public async Task InvokeAsync(HttpContext context, IOrgRolePermissionService orgRolePermissionService, IProjectRolePermissionService projectRolePermissionService, ISysAdminService sysAdminService, IOrganizationService organizationService)
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

    // Check sysadmin status early
    bool isSysAdmin = await sysAdminService.SysAdminCheck(userId);

    // Extract organizationId and projectId(s) from route or query
    int? organizationId = null;
    List<int> projectIds = new List<int>();

    // Try to get organizationId from route
    var routeOrgId = context.GetRouteValue("organizationId")?.ToString();
    if (!string.IsNullOrEmpty(routeOrgId) && int.TryParse(routeOrgId, out var tempOrgId))
    {
        organizationId = tempOrgId;
    }

    // Try to get single projectId from route
    var routeProjectId = context.GetRouteValue("projectId")?.ToString();
    if (!string.IsNullOrEmpty(routeProjectId) && int.TryParse(routeProjectId, out var tempProjectId))
    {
        projectIds.Add(tempProjectId);
    }

    // Try to get multiple projectIds from query parameter
    if (context.Request.Query.TryGetValue("projectIds", out var queryProjectIds))
    {
        foreach (var idValue in queryProjectIds)
        {
            if (!string.IsNullOrEmpty(idValue))
            {
                var ids = idValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var id in ids)
                {
                    if (int.TryParse(id.Trim(), out var parsedId) && !projectIds.Contains(parsedId))
                    {
                        projectIds.Add(parsedId);
                    }
                }
            }
        }
    }
    
    // Non-sysadmins MUST have at least one of organizationId or projectId(s)
    // Sysadmins can access endpoints without org/project IDs
    if (!isSysAdmin && !organizationId.HasValue && !projectIds.Any())
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new
            { error = "Forbidden: Non-admin users require organization or project context" });
        return;
    }
    
    // If org/project IDs ARE provided (even for sysadmins), validate they exist
    long? capturedOrgId = null;
    if (projectIds.Any() || organizationId.HasValue)
    {
        // Check existence for each project and capture the organizationId
        foreach (var projectId in projectIds)
        {
            capturedOrgId = await organizationService.CheckExistence(projectId, organizationId);
        }

        // If no projects but has org, still check org existence
        if (!projectIds.Any() && organizationId.HasValue)
        {
            capturedOrgId = await organizationService.CheckExistence(null, organizationId);
        }

        if (capturedOrgId.HasValue)
        {
            UserContextStorage.OrganizationId = capturedOrgId.Value;
        }
    }

    // Sysadmins skip permission checks
    if (isSysAdmin)
    {
        await _next(context);
        return;
    }

    // Check each permission requirement (only for non-sysadmins)
    foreach (var authAttr in authAttributes)
    {
        bool hasPermission = false;
        
        // Project permissions always take precedence over organization permissions
        if (projectIds.Any())
        {
            // User must have permission in ALL specified projects
            bool hasPermissionInAllProjects = true;

            foreach (var projectId in projectIds)
            {
                var projectPermission = await projectRolePermissionService.PermissionInProject(
                    userId,
                    projectId,
                    authAttr.Action,
                    authAttr.Resource
                );

                if (!projectPermission)
                {
                    hasPermissionInAllProjects = false;
                    break;
                }
            }

            hasPermission = hasPermissionInAllProjects;
        }
        // Only check org permissions when NO project IDs are present
        else if (organizationId.HasValue)
        {
            hasPermission = await orgRolePermissionService.PermissionInOrg(
                userId,
                organizationId.Value,
                authAttr.Action,
                authAttr.Resource
            );
        }

        if (!hasPermission)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Forbidden: User role does not have required permissions in organization or project(s)",
            });
            return;
        }
    }

    // All permission checks passed
    await _next(context);
}
}