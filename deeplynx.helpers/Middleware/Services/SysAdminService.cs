using deeplynx.datalayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace deeplynx.helpers;

//Keeping this interface in the same file as the service
public interface ISysAdminService
{
    Task<bool> SysAdminCheck(long userId);
}

public class SysAdminService : ISysAdminService
{
    private readonly DeeplynxContext _dbContext;
    private readonly ILogger<SysAdminService> _logger;

    public SysAdminService(
        DeeplynxContext dbContext,
        ILogger<SysAdminService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> SysAdminCheck(
        long userId)
    {
        _logger.LogInformation(
            "Checking permission - User: {UserId}",
            userId);

        //check for whether a user has permission to an action/resource within a organization through group membership
        var hasPermission = _dbContext.Database
            .SqlQuery<bool>($@"
             SELECT EXISTS(
                SELECT 1
                FROM deeplynx.users u
                WHERE u.id = {userId}
                  AND u.is_sys_admin = true
                ) as has_permission")
            .AsEnumerable()
            .FirstOrDefault();

        if (hasPermission)
            _logger.LogInformation(
                "Permission granted - User: {UserId}",
                userId);
        else
            _logger.LogWarning(
                "Permission denied - User: {UserId}",
                userId);

        return hasPermission;
    }
}