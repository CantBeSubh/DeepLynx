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
}