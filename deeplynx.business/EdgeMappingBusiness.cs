using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class EdgeMappingBusiness : IEdgeMappingBusiness
{
    private readonly DeeplynxContext _context;

    public EdgeMappingBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EdgeMapping>> GetAllEdgeMappings(long projectId)
    {
        return await _context.EdgeMappings
            .Where(e => e.ProjectId == projectId && e.DeletedAt == null)
            .ToListAsync();
    }

    public async Task<EdgeMapping> GetEdgeMapping(long mappingId)
    {
        return await _context.EdgeMappings
            .FirstOrDefaultAsync(e => e.Id == mappingId && e.DeletedAt == null);
    }

    public async Task<EdgeMapping> CreateEdgeMapping(
        long projectId,
        EdgeMappingRequestDto dto)
    {
        var mapping = new EdgeMapping
        {
            ProjectId = projectId,
            OriginParams = dto.OriginParams.ToString(),
            DestinationParams = dto.DestinationParams.ToString(),
            RelationshipId = dto.RelationshipId,
            OriginId = dto.OriginId,
            DestinationId = dto.DestinationId
        };
        
        _context.EdgeMappings.Add(mapping);
        await _context.SaveChangesAsync();
        
        return mapping;
    }

    public async Task<EdgeMapping> UpdateEdgeMapping(
        long projectId,
        long mappingId,
        EdgeMappingRequestDto dto)
    {
        var mapping = await GetEdgeMapping(mappingId);
        
        mapping.OriginParams = dto.OriginParams.ToString();
        mapping.DestinationParams = dto.DestinationParams.ToString();
        mapping.RelationshipId = dto.RelationshipId;
        mapping.OriginId = dto.OriginId;
        mapping.DestinationId = dto.DestinationId;
        mapping.ProjectId = projectId;
        
        _context.EdgeMappings.Update(mapping);
        await _context.SaveChangesAsync();
        
        return mapping;
    }

    public async Task<bool> DeleteEdgeMapping(long mappingId)
    {
        var mapping = await GetEdgeMapping(mappingId);
        
        mapping.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        
        _context.EdgeMappings.Update(mapping);
        await _context.SaveChangesAsync();

        return true;
    }
    
    /// <summary>
    /// Called primarily by project's delete. Soft delete all edge mappings in a project by project id.
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns>Boolean true on successful deletion.</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<bool> SoftDeleteAllEdgeMappingsByProjectIdAsync(long projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);

        if (project == null)
            throw new KeyNotFoundException("Project not found.");
        
        try
        {
            var edgeMappings = await _context.EdgeMappings.Where(t => t.ProjectId == projectId && t.DeletedAt == null).ToListAsync();
            foreach (var edgeMapping in edgeMappings)
            {
                edgeMapping.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception exception)
        {
            var message = $"An error occurred while deleting project edge mappings: {exception}";
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return false;
        }
    }
}