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

        bool isSysAdmin = await sysAdminService.SysAdminCheck(userId);

        if (!isSysAdmin)
        {
            // Extract both organizationId and projectId from route or query
            int? organizationId = null;
            int? projectId = null;

            // Try to get organizationId
            var routeOrgId = context.GetRouteValue("organizationId")?.ToString();
            if (!string.IsNullOrEmpty(routeOrgId) && int.TryParse(routeOrgId, out var tempOrgId))
            {
                organizationId = tempOrgId;
            }
            // Try to get projectId
            var routeProjectId = context.GetRouteValue("projectId")?.ToString();
            if (!string.IsNullOrEmpty(routeProjectId) && int.TryParse(routeProjectId, out var tempProjectId))
            {
                projectId = tempProjectId;
            }

            // Must have at least one of organizationId or projectId
            if (!organizationId.HasValue && !projectId.HasValue)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                    { error = "Bad Request: Missing both organization AND project ID value" });
                return;
            }

            await organizationService.CheckExistence(projectId, organizationId);
            

           // Check each permission requirement
           foreach (var authAttr in authAttributes)
           {
               bool hasPermission = false;
           
               // Check project permission FIRST if projectId is present
               if (projectId.HasValue)
               {
                   hasPermission = await projectRolePermissionService.PermissionInProject(
                       userId,
                       projectId.Value,
                       authAttr.Action,
                       authAttr.Resource
                   );
                   
                   // If project grants permission, no need to check org
                   if (hasPermission)
                   {
                       continue; // Move to next attribute
                   }
               }
           
               // Only check organization permission if project didn't grant it
               if (organizationId.HasValue)
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
                       error = "Forbidden: User role does not have required permissions in organization or project",
                   });
                   return;
               }
           }
        }

        // All permission checks passed
        await _next(context);
    }

   
}