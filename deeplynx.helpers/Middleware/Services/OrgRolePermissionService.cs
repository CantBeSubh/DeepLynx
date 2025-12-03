using deeplynx.datalayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace deeplynx.helpers;

//Keeping this interface in the same file as the service
public interface IOrgRolePermissionService
{ 
    Task<bool> PermissionInOrg(long userId, long orgId, string action, string resource);
}

public class OrgRolePermissionService : IOrgRolePermissionService
{
    private readonly DeeplynxContext _dbContext;
    private readonly ILogger<OrgRolePermissionService> _logger;

    public OrgRolePermissionService(
        DeeplynxContext dbContext, 
        ILogger<OrgRolePermissionService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> PermissionInOrg(
        long userId, 
        long orgId, 
        string action, 
        string resource)
    {
        _logger.LogInformation(
            "Checking permission - User: {UserId}, Organization: {OrgId}, Action: {Action}, Resource: {Resource}",
            userId, orgId, action, resource);
        
        bool hasPermission = false;
        //check for whether a user has permission to an action/resource within a organization through group membership
        hasPermission = _dbContext.Database
            .SqlQuery<bool>($@"
             SELECT EXISTS(
                SELECT 1
                FROM deeplynx.users u
                LEFT JOIN deeplynx.organization_users ou ON (ou.user_id = u.id)
                WHERE u.id = {userId}
                  AND ou.organization_id = {orgId}
                  AND ou.is_org_admin = true) as has_permission")
            .AsEnumerable()
            .FirstOrDefault();
        if (!hasPermission)
        {
            //check for whether a user has permission to an action/resource within a organization through group membership
            hasPermission = _dbContext.Database
                .SqlQuery<bool>($@"
              SELECT EXISTS(
                SELECT 1
                FROM deeplynx.users u
                LEFT JOIN deeplynx.organization_users ou ON (ou.user_id = u.id)
                LEFT JOIN deeplynx.roles r ON r.organization_id = ou.organization_id
                LEFT JOIN deeplynx.role_permissions rp ON rp.role_id = r.id
                LEFT JOIN deeplynx.permissions perm ON rp.permission_id = perm.id
                WHERE u.id = {userId}
                  AND ou.organization_id = {orgId}
                  AND perm.resource = {resource}
                  AND perm.action = {action}
                  AND r.is_archived = false
                  AND perm.is_archived = false) as has_permission")
                .AsEnumerable()
                .FirstOrDefault();
        }
        if (hasPermission)
        {
            _logger.LogInformation(
                "Permission granted (group) - User: {UserId}, Organization: {OrgId}, Action: {Action}, Resource: {Resource}",
                userId, orgId, action, resource);
        }
        else
        {
            _logger.LogWarning(
                "Permission denied - User: {UserId}, Organization: {OrgId}, Action: {Action}, Resource: {Resource}",
                userId, orgId, action, resource);
        }

        return hasPermission;
    }
    
}