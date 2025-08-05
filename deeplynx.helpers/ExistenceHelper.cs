using System.Linq;
using System.Threading.Tasks;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.helpers
{
    public static class ExistenceHelper
    {
        public static async Task EnsureProjectExistsAsync(DeeplynxContext context, long projectId, bool hideArchived = true)
        {
            var projectExists = hideArchived
                ? await context.Projects.AnyAsync(p => p.Id == projectId && p.ArchivedAt == null)
                : await context.Projects.AnyAsync(p => p.Id == projectId);

            if (!projectExists)
            {
                throw new KeyNotFoundException($"Project with id {projectId} not found");
            }
        }

        public static async Task EnsureDataSourceExistsAsync(DeeplynxContext context, long dataSourceId, bool hideArchived = true)
        {
            var dataSourceExists = hideArchived
                ? await context.DataSources.AnyAsync(ds => ds.Id == dataSourceId && ds.ArchivedAt == null)
                : await context.DataSources.AnyAsync(ds => ds.Id == dataSourceId);

            if (!dataSourceExists)
            {
                throw new KeyNotFoundException($"DataSource with id {dataSourceId} not found");
            }
        }
    }
}