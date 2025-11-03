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
        
        //check for whether a user has permission to an action/resource within a organization through group membership
        var hasPermission = _dbContext.Database
            .SqlQuery<bool>($@"
               SELECT EXISTS (
                    SELECT 1
                    FROM deeplynx.users u
                    LEFT JOIN deeplynx.group_users gu ON gu.user_id = u.id
                    LEFT JOIN deeplynx.groups g ON gu.group_id = g.id
                    LEFT JOIN deeplynx.project_members pm ON (pm.user_id = u.id OR pm.group_id = g.id)
                    LEFT JOIN deeplynx.roles r ON r.id = pm.role_id
                    LEFT JOIN deeplynx.role_permissions rp ON rp.role_id = pm.role_id
                    LEFT JOIN deeplynx.permissions perm ON rp.permission_id = perm.id
                    WHERE u.id = {userId}
                      AND pm.project_id = {orgId}
                      AND perm.resource = {resource}
                      AND perm.action = {action}
                      AND r.is_archived = false
                      AND perm.is_archived = false
                ) AS has_permission")
            .AsEnumerable()
            .FirstOrDefault();

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