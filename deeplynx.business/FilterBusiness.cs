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
    public async Task<IEnumerable<RecordResponseDto>> FilterRecords(FilterRequestDto filterRequest)
    {
        var query = _context.HistoricalRecords.AsQueryable();

        if (!string.IsNullOrEmpty(filterRequest.Name))
        {
            query = query.Where(c => c.Name == filterRequest.Name);
        }

        if (!string.IsNullOrEmpty(filterRequest.Description))
        {
            query = query.Where(c => c.Description == filterRequest.Description);
        }
        
        var records = await query.ToListAsync();

        return records
            .Select(r=>new RecordResponseDto()
            {
                Id = r.Id,
                Uri = r.Uri,
                Properties = r.Properties,
                OriginalId = r.OriginalId,
                Name = r.Name,
                ClassId = r.ClassId,
                DataSourceId = r.DataSourceId,
                ProjectId = r.ProjectId,
                CreatedBy = r.CreatedBy,
                CreatedAt = r.CreatedAt,
                ModifiedBy = r.ModifiedBy,
                ModifiedAt = r.ModifiedAt,
                ArchivedAt = r.ArchivedAt,
                Description = r.Description
            });
    }
}