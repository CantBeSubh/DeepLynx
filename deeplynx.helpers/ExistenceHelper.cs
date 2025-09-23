using System.Linq;
using System.Threading.Tasks;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using deeplynx.helpers.json;
using System.Text.Json.Nodes;

namespace deeplynx.helpers
{
    public static class ExistenceHelper
    {
            public static async Task<ProjectResponseDto> EnsureProjectExistsAsync(
                DeeplynxContext context,
                long projectId,
                ICacheBusiness cacheBusiness,
                bool hideArchived = true)
            {
                if (cacheBusiness == null)
                {
                    throw new KeyNotFoundException("Cache business instance cannot be null.");
                }

                // Try to get the cached list of projects
                var projectResponseList = await cacheBusiness.GetAsync<List<ProjectResponseDto>>("projects");

                if (projectResponseList == null || projectResponseList.Find(p => p.Id == projectId) == null)
                {
                    // Cache is empty, so populate it
                    var projectList = await context.Projects.ToListAsync();

                    projectResponseList = projectList.Select(p => new ProjectResponseDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Abbreviation = p.Abbreviation,
                        IsArchived = p.IsArchived,
                        LastUpdatedAt = p.LastUpdatedAt,
                        LastUpdatedBy = p.LastUpdatedBy,
                        OrganizationId = p.OrganizationId
                    }).ToList();
                    
                    // Store the list in the cache
                    await cacheBusiness.SetAsync("projects", projectResponseList, TimeSpan.FromHours(1));
                }

                // Find the project by ID from the list
                var project = projectResponseList.FirstOrDefault(p => p.Id == projectId);

                if (project == null || hideArchived && project.IsArchived)
                {
                    
                    throw new KeyNotFoundException($"Project with id {projectId} not found.");
                }

                return project;
            }
        
        public static async Task EnsureDataSourceExistsAsync(DeeplynxContext context, long dataSourceId, bool hideArchived = true)
        {
            var dataSourceExists = hideArchived
                ? await context.DataSources.AnyAsync(ds => ds.Id == dataSourceId && ds.IsArchived == false)
                : await context.DataSources.AnyAsync(ds => ds.Id == dataSourceId);

            if (!dataSourceExists)
            {
                throw new KeyNotFoundException($"DataSource with id {dataSourceId} not found");
            }
        }
    }
}