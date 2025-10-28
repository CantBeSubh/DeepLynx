using deeplynx.datalayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace deeplynx.helpers;

//Keeping this interface in the same file as the service
public interface IRolePermissionService
{ 
    Task<bool> UserHasPermissionInProjectAsync(long userId, long projectId, string action, string resource);
}

public class RolePermissionService : IRolePermissionService
{
    private readonly DeeplynxContext _dbContext;
    private readonly ILogger<RolePermissionService> _logger;

    public RolePermissionService(
        DeeplynxContext dbContext, 
        ILogger<RolePermissionService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> UserHasPermissionInProjectAsync(
        long userId, 
        long projectId, 
        string action, 
        string resource)
    {
        _logger.LogInformation(
            "Checking permission - User: {UserId}, Project: {ProjectId}, Action: {Action}, Resource: {Resource}",
            userId, projectId, action, resource);
        
        // Returns TRUE or FALSE based on whether user has action/resource permission for the project
        var hasDirectPermission = await _dbContext.ProjectMembers
            .Include(pm => pm.Role)
                .ThenInclude(r => r.Permissions)
            .Where(pm => 
                pm.UserId == userId && 
                pm.ProjectId == projectId &&
                pm.RoleId != null &&
                !pm.Role.IsArchived)
            .AnyAsync(pm => 
                pm.Role.Permissions.Any(p => 
                    p.Action == action &&
                    p.Resource == resource &&
                    !p.IsArchived));

        if (hasDirectPermission)
        {
            _logger.LogInformation(
                "Permission granted (direct) - User: {UserId}, Project: {ProjectId}, Action: {Action}, Resource: {Resource}",
                userId, projectId, action, resource);
            return true;
        }
        
        //check for whether a user has permission to an action/resource within a project through group membership
        var hasGroupPermission = _dbContext.Database
            .SqlQuery<bool>($@"
                SELECT EXISTS (
                    SELECT 1
                    FROM deeplynx.users u
                    LEFT JOIN deeplynx.group_users gu ON gu.user_id = u.id
                    LEFT JOIN deeplynx.groups g ON gu.group_id = g.id
                    LEFT JOIN deeplynx.project_members pm ON (pm.user_id = u.id OR pm.group_id = g.id)
                    LEFT JOIN deeplynx.role_permissions rp ON rp.role_id = pm.role_id
                    LEFT JOIN deeplynx.permissions perm ON rp.permission_id = perm.id
                    WHERE u.id = {userId}
                      AND pm.project_id = {projectId}
                      AND perm.resource = {resource}
                      AND perm.action = {action}
                ) AS has_permission")
            .AsEnumerable()
            .FirstOrDefault();
        
        var hasPermission = hasGroupPermission;

        if (hasPermission)
        {
            _logger.LogInformation(
                "Permission granted (group) - User: {UserId}, Project: {ProjectId}, Action: {Action}, Resource: {Resource}",
                userId, projectId, action, resource);
        }
        else
        {
            _logger.LogWarning(
                "Permission denied - User: {UserId}, Project: {ProjectId}, Action: {Action}, Resource: {Resource}",
                userId, projectId, action, resource);
        }

        return hasPermission;
    }
    
}