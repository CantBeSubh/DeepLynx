using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using deeplynx.helpers;

namespace deeplynx.business;
/// <summary>
/// Filter record request
/// </summary>
public class FilterBusiness : IFilterBusiness
{
    private readonly DeeplynxContext _context;
    
    /// <summary>
    /// Filter record request
    /// </summary>
    /// <param name="context">The database context to be used for filter operations.</param>
    public FilterBusiness(
        DeeplynxContext context
        )
    {
        _context = context;
    }

    /// <summary>
    /// Filters record request
    /// </summary>
    /// <param name="filterRequest">Filter Request DTO</param>
    /// <returns>A list of record response dtos that match provided filters</returns>
    /// TODO: Partial match with combined strings Example: "Reactor Care" should return entries with "Reactor Core" as the name or description
    public async Task<IEnumerable<HistoricalRecordResponseDto>> FilterRecords(string[] filterRequest)
    {
        var query = _context.HistoricalRecords.AsQueryable();
    
        // Check database for partial match, ignore case
        query = query.Where(c => filterRequest.Any(filter =>
            c.Name.ToLower().Contains(filter.ToLower()) ||
            c.Description.ToLower().Contains(filter.ToLower())));
        
        var records = await query.ToListAsync();

        return records
            .Select(r => new HistoricalRecordResponseDto()
            {
                Id = r.RecordId,
                Uri = r.Uri,
                Properties = r.Properties,
                OriginalId = r.OriginalId,
                Name = r.Name,
                ClassId = r.ClassId,
                ClassName = r.ClassName,
                DataSourceId = r.DataSourceId,
                DataSourceName = r.DataSourceName,
                MappingId = r.MappingId,
                ProjectId = r.ProjectId,
                ProjectName = r.ProjectName,
                Tags = r.Tags,
                CreatedBy = r.CreatedBy,
                CreatedAt = r.CreatedAt,
                ModifiedBy = r.ModifiedBy,
                ModifiedAt = r.ModifiedAt,
                ArchivedAt = r.ArchivedAt,
                Description = r.Description, 
                LastUpdatedAt = r.LastUpdatedAt
            });
    }
}