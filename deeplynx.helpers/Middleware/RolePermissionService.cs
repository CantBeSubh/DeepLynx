using deeplynx.datalayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public interface IRolePermissionService
{ 
    Task<bool> UserHasPermissionInProjectAsync(long userId, long projectId, string action, string resource);
}

// Service implementation using your actual database schema with caching
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
        
        // Check direct user membership
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
        
        var hasGroupPermission = _dbContext.Database
            .SqlQuery<int>($@"
                SELECT COUNT(1)::int
                FROM deeplynx.group_users gu
                INNER JOIN deeplynx.project_members pm ON gu.group_id = pm.group_id
                INNER JOIN deeplynx.roles r ON pm.role_id = r.id
                INNER JOIN deeplynx.role_permissions rp ON r.id = rp.role_id
                INNER JOIN deeplynx.permissions p ON rp.permission_id = p.id
                INNER JOIN deeplynx.groups g ON pm.group_id = g.id
                WHERE gu.user_id = {userId}
                  AND pm.project_id = {projectId}
                  AND p.action = {action}
                  AND p.resource = {resource}
                  AND pm.role_id IS NOT NULL
                  AND r.is_archived = false
                  AND g.is_archived = false
                  AND p.is_archived = false
                LIMIT 1")
            .AsEnumerable()
            .FirstOrDefault() > 0;
        
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