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
    public IEnumerable<HistoricalRecordResponseDto> QueryBuilder(CustomQueryRequestDto[] request, string? textSearch)
    {
        IQueryable<HistoricalRecord> historicalRecords = _context.HistoricalRecords;
        Expression<Func<HistoricalRecord, bool>> predicate = u => true;

        foreach (var query in request)
        {
            if (query.Operator == "KEY_VALUE")
            {
                
                // var contained = JsonSerializer.Deserialize<JsonElement>(query.Json);
                Expression<Func<HistoricalRecord, bool>> next =
                    e => EF.Functions.JsonContains(EF.Property<string>(e, query.Filter),(query.Json!));

                predicate = query.Connector == "OR"
                    ? Or(predicate, next)
                    : And(predicate, next);
            }
            
            if (query.Operator == "LIKE")
            {
                Expression<Func<HistoricalRecord, bool>> next =
                    u => EF.Property<string>(u, query.Filter).Contains(query.Value);

                predicate = query.Connector == "OR"
                    ? Or(predicate, next)
                    : And(predicate, next);
            }

            if (query.Operator == "=")
            {
                if (int.TryParse(query.Value, out var intVal))
                {
                    Expression<Func<HistoricalRecord, bool>> next =
                        u => EF.Property<int>(u, query.Filter) == intVal;

                    predicate = query.Connector == "OR"
                        ? Or(predicate, next)
                        : And(predicate, next);
                }
                else if (DateTime.TryParse(query.Value, out var dateVal))
                {
                    Expression<Func<HistoricalRecord, bool>> next =
                        u => EF.Property<DateTime>(u, query.Filter) == dateVal;

                    predicate = query.Connector == "OR"
                        ? Or(predicate, next)
                        : And(predicate, next);
                }
                else
                {
                    Expression<Func<HistoricalRecord, bool>> next =
                        u => EF.Property<string>(u, query.Filter) == query.Value;

                    predicate = query.Connector == "OR"
                        ? Or(predicate, next)
                        : And(predicate, next);
                }
            }
            if (query.Operator == ">")
            {
                DateTime.TryParse(query.Value, out var dateVal);
                Expression<Func<HistoricalRecord, bool>> next =
                    u => EF.Property<DateTime>(u, query.Filter) > dateVal;

                predicate = query.Connector == "OR"
                    ? Or(predicate, next)
                    : And(predicate, next);
            }
            
            if (query.Operator == "<")
            {
                DateTime.TryParse(query.Value, out var dateVal);
                Expression<Func<HistoricalRecord, bool>> next =
                    u => EF.Property<DateTime>(u, query.Filter) < dateVal;

                predicate = query.Connector == "OR"
                    ? Or(predicate, next)
                    : And(predicate, next);
            }
            
        }
        
        // If we have a full-text query, start from a composable raw SQL that includes JSONB -> text
        if (!string.IsNullOrWhiteSpace(textSearch))
        {
            // NOTE: This is safe from SQL injection because interpolated values are parameterized.
            historicalRecords = _context.HistoricalRecords.FromSqlInterpolated($@"
            SELECT *
            FROM deeplynx.historical_records
            WHERE to_tsvector('english',
                coalesce(name, '') || ' ' ||
                coalesce(description, '') || ' ' ||
                coalesce(class_name, '') || ' ' ||
                coalesce(uri, '') || ' ' ||
                coalesce(original_id, '') || ' ' ||
                coalesce(data_source_name, '') || ' ' ||
                coalesce(project_name, '') || ' ' ||
                coalesce(created_by, '') || ' ' ||
                coalesce(modified_by, '') || ' ' ||
                coalesce(properties::text, '') || ' ' ||
                coalesce(tags::text, '')
            ) @@ websearch_to_tsquery('english', {textSearch})
        ");
        }

        // Compose your dynamic subset on top (EF will AND the WHEREs)
        historicalRecords = historicalRecords.Where(predicate);
       
        return historicalRecords
            .Select(r => new HistoricalRecordResponseDto
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
                LastUpdatedBy = r.LastUpdatedBy,
                IsArchived = r.IsArchived,
                LastUpdatedAt = r.LastUpdatedAt,
                Description = r.Description
            })
            .ToList();
    }
    
     // Combines two lambda expressions (x => condition1, x => condition2)
    // into a single lambda (x => condition1 AND condition2).
    private static Expression<Func<T, bool>> And<T>(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var param = left.Parameters[0];
        var rightBodyWithLeftParam = new Replace(right.Parameters[0], param).Visit(right.Body)!;
        var body = Expression.AndAlso(left.Body, rightBodyWithLeftParam);
        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    // Same as And<T>, but combines with OR instead of AND.
    private static Expression<Func<T, bool>> Or<T>(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var param = left.Parameters[0];
        var rightBodyWithLeftParam = new Replace(right.Parameters[0], param).Visit(right.Body)!;
        var body = Expression.OrElse(left.Body, rightBodyWithLeftParam);
        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    // Walks the expression tree and swaps one ParameterExpression for another.
    class Replace : ExpressionVisitor
    {
        private readonly ParameterExpression from; // the old parameter we want to replace
        private readonly ParameterExpression to;   // the new parameter we want to use

        public Replace(ParameterExpression from, ParameterExpression to)
        {
            this.from = from;
            this.to = to;
        }

        // Whenever the visitor encounters a parameter node,
        // if it's the old one ("from"), replace it with "to".
        protected override Expression VisitParameter(ParameterExpression node)
            => node == from ? to : node;
    }
    
    
    /// <summary>
    /// Full text records search
    /// </summary>
    /// <param name="userQuery">String query</param>
    /// <returns>A list of historical record response dtos that match provided query parameters</returns>
    public async Task<IEnumerable<FullTextQueryResponseDto>> Search(string userQuery)
    {
        if (string.IsNullOrWhiteSpace(userQuery))
            throw new Exception("Search query is required.");
        
        // full text search query for all text properties of historical records table 
        var sql = @"
        WITH tag_name AS (
          SELECT rt.record_id,
                 string_agg(DISTINCT coalesce(t.name,''), ' ') AS tags_text
          FROM deeplynx.record_tags rt
          JOIN deeplynx.tags t ON t.id = rt.tag_id
          GROUP BY rt.record_id
        )
        SELECT
          r.*,
          c.name  AS ClassName,
          ds.name AS DataSourceName,
          p.name  AS ProjectName,
          tn.tags_text AS Tags,
          r.original_id AS OriginalId
        FROM deeplynx.records r
        LEFT JOIN deeplynx.classes      c  ON c.id  = r.class_id
        LEFT JOIN deeplynx.data_sources ds ON ds.id = r.data_source_id
        LEFT JOIN deeplynx.projects     p  ON p.id  = r.project_id
        LEFT JOIN tag_name              tn ON tn.record_id = r.id
        WHERE
          to_tsvector('english',
              coalesce(r.name, '') || ' ' ||
              coalesce(r.description, '') || ' ' ||
              coalesce(c.name, '') || ' ' ||
              coalesce(r.uri, '') || ' ' ||
              coalesce(r.original_id, '') || ' ' ||
              coalesce(ds.name, '') || ' ' ||
              coalesce(p.name, '') || ' ' ||
              coalesce(r.created_by, '') || ' ' ||
              coalesce(r.modified_by, '') || ' ' ||
              coalesce(r.properties::text, '') || ' ' ||
              coalesce(tn.tags_text, '')
          ) @@ to_tsquery('english', @query);
        ";

        var param = new NpgsqlParameter("query", userQuery);

        var results = await _context.Database
            .SqlQueryRaw<FullTextQueryResponseDto>(sql, param)
            .ToListAsync();
        
        return results
            .Select(r => new FullTextQueryResponseDto()
            {
                    Uri = r.Uri,
                    Properties = r.Properties,
                    OriginalId = r.OriginalId,
                    Name = r.Name,
                    ClassName = r.ClassName,
                    DataSourceName = r.DataSourceName,
                    ProjectName = r.ProjectName,
                    Tags = r.Tags,
                    Description = r.Description
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
            classes = classes.Where(c => c.ArchivedAt == null).ToList();
        }
        
        return classes 
            .Select(c => new ClassResponseDto()
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Uuid = c.Uuid,
                ProjectId = c.ProjectId,
                CreatedBy = c.CreatedBy,
                CreatedAt = c.CreatedAt,
                ModifiedBy = c.ModifiedBy,
                ModifiedAt = c.ModifiedAt,
                ArchivedAt = c.ArchivedAt,
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
            dataSources = dataSources.Where(d => d.ArchivedAt == null).ToList();
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
                CreatedBy = d.CreatedBy,
                CreatedAt = d.CreatedAt,
                ModifiedBy = d.ModifiedBy,
                ModifiedAt = d.ModifiedAt,
                ArchivedAt = d.ArchivedAt,
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
            tagQuery = tagQuery.Where(t => t.ArchivedAt == null);
        }
            
        return await tagQuery.Select(t => new TagResponseDto()
            {
                Id = t.Id,
                Name = t.Name,
                ProjectId = t.ProjectId,
                CreatedBy = t.CreatedBy,
                CreatedAt = t.CreatedAt,
                ModifiedBy = t.ModifiedBy,
                ModifiedAt = t.ModifiedAt,
                ArchivedAt = t.ArchivedAt,
            })
            .ToListAsync();
    }

}
