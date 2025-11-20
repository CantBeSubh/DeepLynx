using deeplynx.datalayer.Models;
using deeplynx.helpers.Context;
using deeplynx.interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace deeplynx.helpers;

public interface IOrganizationService
{
    Task CheckExistence(long? projectId, long? organizationId);
}
public class OrganizationService : IOrganizationService
{
    private readonly DeeplynxContext _dbContext;
    private readonly ILogger<SysAdminService> _logger;
    private readonly ICacheBusiness _cacheBusiness;

    public OrganizationService(
        DeeplynxContext dbContext,
        ILogger<SysAdminService> logger, ICacheBusiness cacheBusiness)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cacheBusiness = cacheBusiness;
    }
    
    public async Task CheckExistence(long? projectId, long? organizationId)
    {
        if (organizationId.HasValue)
        {
            await ExistenceHelper.EnsureOrganizationExistsAsync(_dbContext, organizationId.Value);
        }

        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_dbContext, projectId.Value, _cacheBusiness);
            _logger.LogInformation(
                "Grabbing organization from project - Project: {ProjectId}",
                projectId);
            
            var project = _dbContext.Projects.FirstOrDefault(
                p => p.Id == projectId
                && (!organizationId.HasValue || p.OrganizationId == organizationId.Value)
            );

            if (project != null)
            {
                _logger.LogInformation(
                    "Organization found - Organization: {OrganizationId}",
                    project.OrganizationId);
                UserContextStorage.OrganizationId = project.OrganizationId;
            }
            else
                _logger.LogWarning(
                    "No organization associated with project ID - Project: {ProjectId}",
                    projectId);
        }
    }
}