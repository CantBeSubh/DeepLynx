using System.Linq.Expressions;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.interfaces;
using deeplynx.models;
using DuckDB.NET.Native;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace deeplynx.business;

/// <summary>
/// Filter record request
/// </summary>
public class QueryBusiness : IQueryBusiness
{
    private readonly DeeplynxContext _context;

    /// <summary>
    /// Filter record request
    /// </summary>
    /// <param name="context">The database context to be used for filter operations.</param>
    public QueryBusiness(
        DeeplynxContext context
    )
    {
        _context = context;
    }
    
    /// <summary>
    /// Build a query
    /// </summary>
    /// <param name="request">Array of query component dtos, initial connector string will be null</param>
    /// <param name="textSearch">Full text search phrase</param>
    /// <returns>A list of historical record response dtos that match provided filters</returns>
public IEnumerable<HistoricalRecordResponseDto> QueryBuilder(CustomQueryRequestDto[] request, string? textSearch = null)
{
    if (request == null)
    {
        throw new Exception("Custom query request dto cannot be null");
    }

    // Build the WHERE conditions for your custom queries
    var whereConditions = new List<string>();
    var sqlParameters = new List<object>();
    var parameterIndex = 1; 

    if (request?.Length > 0)
    {
        foreach (var query in request)
        {
            string condition = "";
            
            if (query.Operator == "KEY_VALUE")
            {
                condition = $"({query.Filter}::jsonb @> ${parameterIndex}::jsonb)";
                sqlParameters.Add(query.Json);
                parameterIndex++;
            }
            else if (query.Operator == "LIKE")
            {
                condition = $"hr.{query.Filter} ILIKE ${parameterIndex}";
                sqlParameters.Add($"%{query.Value}%");
                parameterIndex++;
            }
            else if (query.Operator == "=")
            {
                if (int.TryParse(query.Value, out var intVal))
                {
                    condition = $"hr.{query.Filter} = ${parameterIndex}";
                    sqlParameters.Add(intVal);
                    parameterIndex++;
                }
                else if (DateTime.TryParse(query.Value, out var dateVal))
                {
                    condition = $"hr.{query.Filter} = ${parameterIndex}";
                    sqlParameters.Add(dateVal);
                    parameterIndex++;
                }
                else
                {
                    condition = $"hr.{query.Filter} = ${parameterIndex}";
                    sqlParameters.Add(query.Value);
                    parameterIndex++;
                }
            }
            else if (query.Operator == ">")
            {
                if (DateTime.TryParse(query.Value, out var dateVal))
                {
                    condition = $"hr.{query.Filter} > ${parameterIndex}";
                    sqlParameters.Add(dateVal);
                    parameterIndex++;
                }
            }
            else if (query.Operator == "<")
            {
                if (DateTime.TryParse(query.Value, out var dateVal))
                {
                    condition = $"hr.{query.Filter} < ${parameterIndex}";
                    sqlParameters.Add(dateVal);
                    parameterIndex++;
                }
            }

            if (!string.IsNullOrEmpty(condition))
            {
                if (whereConditions.Count > 0)
                {
                    var connector = query.Connector?.ToUpper() == "OR" ? " OR " : " AND ";
                    whereConditions.Add(connector + condition);
                }
                else
                {
                    whereConditions.Add(condition);
                }
            }
        }
    }

    var additionalWhere = whereConditions.Count > 0 ? 
        " AND (" + string.Join("", whereConditions) + ")" : "";

    var baseSql = $@"
        SELECT DISTINCT ON (hr.record_id)
            hr.*,
            hr.class_id as ClassId,
            hr.class_name as ClassName,
            hr.original_id as OriginalId,
            hr.data_source_name as DataSourceName,
            hr.data_source_id as DataSourceId,
            hr.project_name as ProjectName,
            hr.project_id as ProjectId,
            hr.last_updated_at as LastUpdatedAt,
            hr.last_updated_by as LastUpdatedBy,
            hr.object_storage_name as ObjectStorageName,
            hr.object_storage_id as ObjectStorageId,
            hr.record_id as RecordId,
            hr.is_archived as IsArchived
        FROM deeplynx.historical_records hr
        WHERE hr.is_archived = false
        {additionalWhere}";

    IQueryable<HistoricalRecord> historicalRecords;

    if (!string.IsNullOrWhiteSpace(textSearch))
    {
        var textSearchSql = baseSql + $@"
            AND to_tsvector('english',
                    coalesce(name, '') || ' ' ||
                    coalesce(description, '') || ' ' ||
                    coalesce(class_name, '') || ' ' ||
                    coalesce(uri, '') || ' ' ||
                    coalesce(original_id, '') || ' ' ||
                    coalesce(data_source_name, '') || ' ' ||
                    coalesce(project_name, '') || ' ' ||
                    coalesce(properties::text, '') || ' ' ||
                    coalesce(tags::text, '')
                )@@ websearch_to_tsquery('english', ${parameterIndex})
            ORDER BY hr.record_id, hr.last_updated_at DESC";
            
        sqlParameters.Add(textSearch);
        historicalRecords = _context.HistoricalRecords.FromSqlRaw(textSearchSql, sqlParameters.ToArray());
    }
    else
    {
        var finalSql = baseSql + " ORDER BY hr.record_id, hr.last_updated_at DESC";
        
        if (sqlParameters.Count > 0)
        {
          
            if (sqlParameters.Count == 1)
            {
                var param1 = sqlParameters[0];
                historicalRecords = _context.HistoricalRecords.FromSqlInterpolated($@"
                    SELECT DISTINCT ON (hr.record_id)
                        hr.*,
                        hr.class_id as ClassId,
                        hr.class_name as ClassName,
                        hr.original_id as OriginalId,
                        hr.data_source_name as DataSourceName,
                        hr.data_source_id as DataSourceId,
                        hr.project_name as ProjectName,
                        hr.project_id as ProjectId,
                        hr.last_updated_at as LastUpdatedAt,
                        hr.last_updated_by as LastUpdatedBy,
                        hr.object_storage_name as ObjectStorageName,
                        hr.object_storage_id as ObjectStorageId,
                        hr.record_id as RecordId,
                        hr.is_archived as IsArchived
                    FROM deeplynx.historical_records hr
                    WHERE hr.is_archived = false
                     AND (hr.original_id = {param1}) ORDER BY hr.record_id, hr.last_updated_at DESC");
            }
            else
            {
                historicalRecords = _context.HistoricalRecords.FromSqlRaw(finalSql, sqlParameters.ToArray());
            }
        }
        else
        {
            historicalRecords = _context.HistoricalRecords.FromSqlRaw(finalSql);
        }
    }

    return historicalRecords
        .Select(r => new HistoricalRecordResponseDto
        {
            Id = r.RecordId,
            Uri = r.Uri,
            Properties = r.Properties,
            OriginalId = r.OriginalId,
            Name = r.Name,
            Description = r.Description,
            ClassId = r.ClassId,
            ClassName = r.ClassName,
            DataSourceId = r.DataSourceId,
            DataSourceName = r.DataSourceName,
            ObjectStorageId = r.ObjectStorageId,
            ObjectStorageName = r.ObjectStorageName,
            ProjectId = r.ProjectId,
            ProjectName = r.ProjectName,
            Tags = r.Tags,
            LastUpdatedBy = r.LastUpdatedBy,
            LastUpdatedAt = r.LastUpdatedAt
        }).ToList();
}
    
    
    
    /// <summary>
    /// Full text records search
    /// </summary>
    /// <param name="userQuery">String query</param>
    /// <returns>A list of historical record response dtos that match provided query parameters</returns>
    public async Task<IEnumerable<HistoricalRecordResponseDto>> Search(string userQuery)
    {
        if (string.IsNullOrWhiteSpace(userQuery))
            throw new Exception("Search query is required.");
        
        var sql = @"
            SELECT DISTINCT ON (hr.record_id)
            hr.*,
            hr.class_id as ClassId,
            hr.class_name as ClassName,
            hr.original_id as OriginalId,
            hr.data_source_name as DataSourceName,
            hr.data_source_id as DataSourceId,
            hr.project_name as ProjectName,
            hr.project_id as ProjectId,
            hr.last_updated_at as LastUpdatedAt,
            hr.last_updated_by as LastUpdatedBy,
            hr.object_storage_name as ObjectStorageName,
            hr.object_storage_id as ObjectStorageId,
            hr.record_id as RecordId,
            hr.is_archived as IsArchived
        FROM deeplynx.historical_records hr
        WHERE hr.is_archived = false
        AND to_tsvector('english',
                coalesce(name, '') || ' ' ||
                coalesce(description, '') || ' ' ||
                coalesce(class_name, '') || ' ' ||
                coalesce(uri, '') || ' ' ||
                coalesce(original_id, '') || ' ' ||
                coalesce(data_source_name, '') || ' ' ||
                coalesce(project_name, '') || ' ' ||
                coalesce(properties::text, '') || ' ' ||
                coalesce(tags::text, '')
            )@@ websearch_to_tsquery('english',@query)
        ORDER BY hr.record_id, hr.last_updated_at DESC;";

        var param = new NpgsqlParameter("query", userQuery);

        var results = await _context.Database
            .SqlQueryRaw<HistoricalRecordResponseDto>(sql, param)
            .ToListAsync();
        
        return results
            .Select(r => new HistoricalRecordResponseDto()
            {
                Uri = r.Uri,
                Properties = r.Properties,
                OriginalId = r.OriginalId,
                Name = r.Name,
                Description = r.Description,
                ClassId = r.ClassId,
                ClassName = r.ClassName,
                DataSourceId = r.DataSourceId,
                DataSourceName = r.DataSourceName,
                ObjectStorageId = r.ObjectStorageId,
                ObjectStorageName = r.ObjectStorageName,
                ProjectId = r.ProjectId,
                ProjectName = r.ProjectName,
                Tags = r.Tags,
                LastUpdatedBy = r.LastUpdatedBy,
                LastUpdatedAt = r.LastUpdatedAt
            });
    }
    
    
    /// <summary>
    /// Retrieves all classes for specific projects.
    /// </summary>
    /// <param name="projectIds">The IDs of the projects whose data sources are to be retrieved</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived data sources from the result</param>
    /// <returns>A list of data sources within the given project.</returns>
    public async Task<List<ClassResponseDto>> GetAllClasses(long[] projectIds, bool hideArchived)
    {
        foreach (var projectId in projectIds){
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, hideArchived);
        }

        var classes = await _context.Classes
            .Where(c => projectIds.Contains(c.ProjectId)).ToListAsync();
        
        if (hideArchived)
        {
            classes = classes.Where(c => !c.IsArchived).ToList();
        }
        
        return classes 
            .Select(c => new ClassResponseDto()
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Uuid = c.Uuid,
                ProjectId = c.ProjectId,
                LastUpdatedAt = c.LastUpdatedAt,
                LastUpdatedBy = c.LastUpdatedBy,
                IsArchived = c.IsArchived,

            }).ToList();
    }
    
    /// <summary>
    /// Retrieves all data sources for a specific project.
    /// </summary>
    /// <param name="projectIds">The IDs of the projects whose data sources are to be retrieved</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived data sources from the result</param>
    /// <returns>A list of data sources within the given project.</returns>
    public async Task<List<DataSourceResponseDto>> GetAllDataSources(long[] projectIds, bool hideArchived)
    {
        foreach (var projectId in projectIds)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, hideArchived);
        }
            
        var dataSources = await _context.DataSources
            .Where(d => projectIds.Contains(d.ProjectId)).ToListAsync();

        if (hideArchived)
        {
            dataSources = dataSources.Where(d =>  !d.IsArchived).ToList();
        }
            
        return dataSources
            .Select(d => new DataSourceResponseDto()
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Default = d.Default,
                Abbreviation = d.Abbreviation,
                Type = d.Type,
                BaseUri = d.BaseUri,
                // return empty object for config if null
                Config = JsonNode.Parse(d.Config ?? "{}") as JsonObject,
                ProjectId = d.ProjectId,
                LastUpdatedAt = d.LastUpdatedAt,
                LastUpdatedBy = d.LastUpdatedBy,
                IsArchived = d.IsArchived,

            }).ToList();
    }
    
    /// <summary>
    /// Retrieves all tags for a specified project.
    /// </summary>
    /// <param name="projectIds">The IDs of the projects whose tags are to be retrieved.</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived tags from the result</param>
    /// <returns>A list of tags belonging to the project.</returns>
    public async Task<List<TagResponseDto>> GetAllTags(long[] projectIds, bool hideArchived)
    {
        foreach (var projectId in projectIds)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, hideArchived);
        }
        var tagQuery = _context.Tags
            .Where(t => projectIds.Contains(t.ProjectId));
            
        if (hideArchived)
        {
            tagQuery = tagQuery.Where(t => !t.IsArchived);
        }
            
        return await tagQuery.Select(t => new TagResponseDto()
            {
                Id = t.Id,
                Name = t.Name,
                ProjectId = t.ProjectId,
                LastUpdatedBy = t.LastUpdatedBy,
                LastUpdatedAt = t.LastUpdatedAt,
            })
            .ToListAsync();
    }

}
