using deeplynx.datalayer.Models;
using deeplynx.helpers.Context;
using deeplynx.interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace deeplynx.helpers;

public interface IOrganizationService
{
    Task<long> CheckExistence(long? projectId, long? organizationId);
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
    
    public async Task<long> CheckExistence(long? projectId, long? organizationId)
    {
        if (organizationId.HasValue)
        {
            await ExistenceHelper.EnsureOrganizationExistsAsync(_dbContext, organizationId.Value);
        }

        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_dbContext, projectId.Value, _cacheBusiness);
        
            var project = await _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                throw new KeyNotFoundException($"Project with ID {projectId} not found");
            }

            if (organizationId.HasValue && project.OrganizationId != organizationId.Value)
            {
                throw new InvalidOperationException(
                    $"Project {projectId} does not belong to organization {organizationId.Value}");
            }

            _logger.LogInformation("Organization found - Organization: {OrganizationId}", project.OrganizationId);
        
            // Return the organizationId
            return project.OrganizationId;
        }
        
        return organizationId.GetValueOrDefault();
    }
}