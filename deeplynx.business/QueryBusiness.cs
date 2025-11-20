using System.Text.Json;
using System.Text.Json.Nodes;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace deeplynx.business;

/// <summary>
///     Filter record request
/// </summary>
public class QueryBusiness : IQueryBusiness
{
    private readonly ICacheBusiness _cache;
    private readonly DeeplynxContext _context;

    /// <summary>
    ///     Filter record request
    /// </summary>
    /// <param name="context">The database context to be used for filter operations.</param>
    public QueryBusiness(DeeplynxContext context, ICacheBusiness? cacheBusiness)
    {
        _context = context;
        _cache = cacheBusiness;
    }

    /// <summary>
    ///     Build a query
    /// </summary>
    /// <param name="request">Array of query component dtos, initial connector string will be null</param>
    /// <param name="textSearch">Full text search phrase</param>
    /// ///
    /// <param name="projectIds">Project ids that a user has access to</param>
    /// <returns>A list of historical record response dtos that match provided filters</returns>
    public async Task<IEnumerable<HistoricalRecordResponseDto>> QueryBuilder(
        CustomQueryDtos.CustomQueryRequestDto[] request, long[] projectIds, string? textSearch = null)
    {
        if (request == null) throw new ArgumentException("Custom query request dto cannot be null");

        try
        {
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
                AND hr.project_id = ANY(@projectIds)";

            var parameters = new List<NpgsqlParameter>();
            var conditions = new List<string>();
            var projectIdsParam = new NpgsqlParameter("projectIds", NpgsqlDbType.Array | NpgsqlDbType.Bigint)
            {
                Value = projectIds
            };
            parameters.Add(projectIdsParam);

            // Build individual conditions
            if (request?.Length > 0)
                for (var i = 0; i < request.Length; i++)
                {
                    var query = request[i];
                    if (string.IsNullOrWhiteSpace(query.Value) && query.Operator != "KEY_VALUE")
                        throw new ArgumentException("Value cannot be null or empty.");
                    var condition = "";
                    var paramName = $"param{i}";

                    // Build the individual condition
                    if (query.Operator == "KEY_VALUE")
                    {
                        condition = $"({query.Filter}::jsonb @> @{paramName}::jsonb)";
                        parameters.Add(new NpgsqlParameter(paramName, query.Json));
                    }
                    else if (query.Operator == "LIKE")
                    {
                        // Check if this is a JSONB column that needs special handling
                        var jsonbColumns = new[] { "properties", "tags" };

                        if (jsonbColumns.Contains(query.Filter.ToLower()))
                            // For JSONB columns, convert to text and search
                            condition = $"jsonb_pretty(hr.{query.Filter}) ILIKE @{paramName}";
                        else
                            condition = $"hr.{query.Filter} ILIKE @{paramName}";
                        parameters.Add(new NpgsqlParameter(paramName, $"%{query.Value}%"));
                    }
                    else if (query.Operator == "=")
                    {
                        // Check if this is a JSONB column that needs special handling
                        var jsonbColumns = new[] { "properties", "tags" };

                        if (jsonbColumns.Contains(query.Filter.ToLower()))
                        {
                            // For JSONB columns, convert to text for exact match
                            condition = $"jsonb_pretty(hr.{query.Filter}) ILIKE @{paramName}";
                            parameters.Add(new NpgsqlParameter(paramName, $"%{query.Value}%"));
                        }
                        else
                        {
                            condition = $"hr.{query.Filter} = @{paramName}";

                            if (int.TryParse(query.Value, out var intVal))
                                parameters.Add(new NpgsqlParameter(paramName, intVal));
                            else if (DateTime.TryParse(query.Value, out var dateVal))
                                parameters.Add(new NpgsqlParameter(paramName, dateVal));
                            else
                                parameters.Add(new NpgsqlParameter(paramName, query.Value));
                        }
                    }
                    else if (query.Operator == ">")
                    {
                        condition = $"hr.{query.Filter} > @{paramName}";

                        if (DateTime.TryParse(query.Value, out var dateVal))
                            parameters.Add(new NpgsqlParameter(paramName, dateVal));
                        else
                            parameters.Add(new NpgsqlParameter(paramName, query.Value));
                    }
                    else if (query.Operator == "<")
                    {
                        condition = $"hr.{query.Filter} < @{paramName}";

                        if (DateTime.TryParse(query.Value, out var dateVal))
                            parameters.Add(new NpgsqlParameter(paramName, dateVal));
                        else
                            parameters.Add(new NpgsqlParameter(paramName, query.Value));
                    }
                    else
                    {
                        throw new ArgumentException("Invalid operator in query.");
                    }

                    if (!string.IsNullOrEmpty(condition)) conditions.Add(condition);
                }

            if (conditions.Any())
            {
                sql += " AND (";

                for (var i = 0; i < conditions.Count; i++)
                {
                    if (i > 0)
                    {
                        var connector = request[i].Connector?.ToUpper() == "OR" ? " OR " : " AND ";
                        sql += connector;
                    }

                    sql += conditions[i];
                }

                sql += ")";
            }

            if (!string.IsNullOrWhiteSpace(textSearch))
            {
                // Split query into words and add :* to each for prefix matching
                var processedQuery = string.Join(" & ",
                    textSearch.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(word => word.Trim() + ":*"));
                var processedQueryParam = new NpgsqlParameter("processedQuery", processedQuery);
                var originalQueryParam = new NpgsqlParameter("originalQuery", textSearch);
                parameters.Add(processedQueryParam);
                parameters.Add(originalQueryParam);

                var textSearchCondition = @"
                    AND (
                        to_tsvector('english',
                                coalesce(name, '') || ' ' ||
                                coalesce(description, '') || ' ' ||
                                coalesce(class_name, '') || ' ' ||
                                coalesce(uri, '') || ' ' ||
                                coalesce(original_id, '') || ' ' ||
                                coalesce(data_source_name, '') || ' ' ||
                                coalesce(project_name, '') || ' ' ||
                                coalesce(properties::text, '') || ' ' ||
                                coalesce(tags::text, '')
                            )@@ to_tsquery('english', @processedQuery)
                        OR hr.name ILIKE '%' || @originalQuery || '%'
                        OR hr.description ILIKE '%' || @originalQuery || '%'
                        OR hr.original_id ILIKE '%' || @originalQuery || '%'
                        OR hr.data_source_name ILIKE '%' || @originalQuery || '%'
                        OR hr.project_name ILIKE '%' || @originalQuery || '%'
                        OR hr.class_name ILIKE '%' || @originalQuery || '%'
                    )";

                sql += textSearchCondition;
            }

            // Add ORDER BY
            sql += " ORDER BY hr.record_id, hr.last_updated_at DESC";

            // Execute the query with parameters
            var historicalRecords = _context.HistoricalRecords.FromSqlRaw(sql, parameters.ToArray());

            return await historicalRecords
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
                }).ToListAsync();
        }
        catch (PostgresException ex) when (ex.SqlState == "42703") // undefined_column
        {
            throw new ArgumentException(
                "Invalid column name in query. Please check your filter fields against the historical_records table structure.",
                ex);
        }
        catch (PostgresException ex) when (ex.SqlState == "42601") // syntax_error
        {
            throw new ArgumentException("Invalid query syntax. Please check your operators and values.", ex);
        }
        catch (PostgresException ex) when (ex.SqlState == "22P02")
        {
            throw new ArgumentException(
                "Invalid data type in query. Please check that your values match the expected column data types.", ex);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format in KEY_VALUE operation: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Error executing query: {ex.Message}", ex);
        }
    }

    /// <summary>
    ///     Full text records search
    /// </summary>
    /// <param name="userQuery">String query</param>
    /// <param name="projectIds">Project ids that a user has access to</param>
    /// <returns>A list of historical record response dtos that match provided query parameters</returns>
    public async Task<IEnumerable<HistoricalRecordResponseDto>> Search(string userQuery, long[] projectIds)
    {
        if (string.IsNullOrWhiteSpace(userQuery))
            throw new Exception("Search query is required.");

        // Process query for full-text search (prefix matching)
        var processedQuery = string.Join(" & ",
            userQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word.Trim() + ":*"));

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
        AND hr.project_id = ANY(@project_ids)
        AND (
            to_tsvector('english',
                    coalesce(name, '') || ' ' ||
                    coalesce(description, '') || ' ' ||
                    coalesce(class_name, '') || ' ' ||
                    coalesce(uri, '') || ' ' ||
                    coalesce(original_id, '') || ' ' ||
                    coalesce(data_source_name, '') || ' ' ||
                    coalesce(project_name, '') || ' ' ||
                    coalesce(properties::text, '') || ' ' ||
                    coalesce(tags::text, '')
                )@@ to_tsquery('english', @processed_query)
            OR hr.name ILIKE '%' || @original_query || '%'
            OR hr.description ILIKE '%' || @original_query || '%'
            OR hr.original_id ILIKE '%' || @original_query || '%'
            OR hr.data_source_name ILIKE '%' || @original_query || '%'
            OR hr.project_name ILIKE '%' || @original_query || '%'
            OR hr.class_name ILIKE '%' || @original_query || '%'
        )
        ORDER BY hr.record_id, hr.last_updated_at DESC";

        var param1 = new NpgsqlParameter("processed_query", processedQuery);
        var param2 = new NpgsqlParameter("original_query", userQuery);
        var param3 = new NpgsqlParameter("project_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint)
        {
            Value = projectIds
        };

        var results = _context.HistoricalRecords.FromSqlRaw(sql, param1, param2, param3);

        return await results
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
            }).ToListAsync();
    }

    /// <summary>
    ///     Retrieves all classes for specific projects.
    /// </summary>
    /// <param name="projectIds">The IDs of the projects whose data sources are to be retrieved</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived data sources from the result</param>
    /// <returns>A list of data sources within the given project.</returns>
    public async Task<List<ClassResponseDto>> GetAllClasses(long[] projectIds, bool hideArchived)
    {
        foreach (var projectId in projectIds)
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cache, hideArchived);

        var classes = await _context.Classes
            .Where(c => projectIds.Contains(c.ProjectId)).ToListAsync();

        if (hideArchived) classes = classes.Where(c => !c.IsArchived).ToList();

        return classes
            .Select(c => new ClassResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Uuid = c.Uuid,
                ProjectId = c.ProjectId,
                LastUpdatedAt = c.LastUpdatedAt,
                LastUpdatedBy = c.LastUpdatedBy,
                IsArchived = c.IsArchived
            }).ToList();
    }

    /// <summary>
    ///     Retrieves all data sources for a specific project.
    /// </summary>
    /// <param name="projectIds">The IDs of the projects whose data sources are to be retrieved</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived data sources from the result</param>
    /// <returns>A list of data sources within the given project.</returns>
    public async Task<List<DataSourceResponseDto>> GetAllDataSources(long[] projectIds, bool hideArchived)
    {
        foreach (var projectId in projectIds)
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cache, hideArchived);

        var dataSources = await _context.DataSources
            .Where(d => projectIds.Contains(d.ProjectId)).ToListAsync();

        if (hideArchived) dataSources = dataSources.Where(d => !d.IsArchived).ToList();

        return dataSources
            .Select(d => new DataSourceResponseDto
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
                IsArchived = d.IsArchived
            }).ToList();
    }

    /// <summary>
    ///     Retrieves all tags for a specified project.
    /// </summary>
    /// <param name="projectIds">The IDs of the projects whose tags are to be retrieved.</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived tags from the result</param>
    /// <returns>A list of tags belonging to the project.</returns>
    public async Task<List<TagResponseDto>> GetAllTags(long[] projectIds, bool hideArchived)
    {
        foreach (var projectId in projectIds)
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cache, hideArchived);
        var tagQuery = _context.Tags
            .Where(t => projectIds.Contains(t.ProjectId));

        if (hideArchived) tagQuery = tagQuery.Where(t => !t.IsArchived);

        return await tagQuery.Select(t => new TagResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                ProjectId = t.ProjectId,
                LastUpdatedBy = t.LastUpdatedBy,
                LastUpdatedAt = t.LastUpdatedAt
            })
            .ToListAsync();
    }


    /// <summary>
    ///     Save search for user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="textSearch">Full text search string</param>
    /// <param name="filters">Query filter object array</param>
    /// <param name="favorite">Boolean for if favorite</param>
    /// <param name="alias">Name for saved search</param>
    /// <returns>True if successfully saved</returns>
    public async Task<bool> SaveSearch(long userId, string alias, string textSearch,
        CustomQueryDtos.CustomQueryRequestDto[] filters, bool favorite = false)
    {
        if (filters == null)
            throw new ArgumentNullException(nameof(filters), "Query filters cannot be null");
        // Create an object that wraps both the textSearch and filters array
        var searchData = new CustomQueryDtos.CustomQueryResponseDto
        {
            textSearch = textSearch,
            Filter = filters
        };

        var queryBuilt = JsonSerializer.Serialize(searchData);
        var savedSearch = new SavedSearch
        {
            Name = alias,
            Search = queryBuilt,
            IsFavorite = favorite,
            UserId = userId
        };
        _context.SavedSearches.Add(savedSearch);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    ///     Get saved searches
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>List of saved searches for the user</returns>
    public async Task<List<CustomQueryDtos.CustomQueryResponseDto>> GetSavedSearches(long userId)
    {
        var savedSearches = await _context.SavedSearches
            .Where(s => s.UserId == userId)
            .ToListAsync();

        var result = new List<CustomQueryDtos.CustomQueryResponseDto>();

        foreach (var search in savedSearches)
        {
            // Deserialize the JSON string back to the original structure
            var searchData = JsonSerializer.Deserialize<CustomQueryDtos.CustomQueryResponseDto>(search.Search);

            Console.WriteLine($"Filters count: {searchData?.Filter?.Length ?? 0}");

            result.Add(new CustomQueryDtos.CustomQueryResponseDto
            {
                textSearch = searchData?.textSearch,
                Filter = searchData?.Filter
            });
        }

        return result;
    }

    /// <summary>
    ///     Retrieves current records for projects, ordered by last_updated_at first
    /// </summary>
    /// <param name="projectIds">An array of project ids</param>
    /// <returns>An array of records</returns>
    public async Task<IEnumerable<HistoricalRecordResponseDto>> GetRecentlyAddedRecords(long[] projectIds)
    {
        var records = _context.HistoricalRecords
            .Where(p => projectIds.Contains(p.ProjectId))
            .Where(r => !r.IsArchived)
            .GroupBy(r => r.RecordId)
            .Select(g => g.OrderByDescending(r => r.LastUpdatedAt).First())
            .ToList();

        return records
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
                ProjectId = r.ProjectId,
                ProjectName = r.ProjectName,
                Tags = r.Tags,
                Description = r.Description,
                LastUpdatedBy = r.LastUpdatedBy,
                IsArchived = r.IsArchived,
                LastUpdatedAt = r.LastUpdatedAt
            });
    }


    /// <summary>
    ///     Retrieves all records for multiple projects.
    /// </summary>
    /// <param name="projects">Array of project ids whose records are to be retrieved</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived records from the result</param>
    /// <returns>A list of records based on the applied filters.</returns>
    public async Task<IEnumerable<HistoricalRecordResponseDto>> GetMultiProjectRecords(
        long[] projects, bool hideArchived)
    {
        var recordQuery = _context.HistoricalRecords
            .Where(r => projects.Contains(r.ProjectId));

        if (hideArchived) recordQuery = recordQuery.Where(r => !r.IsArchived);

        var records = await recordQuery
            .GroupBy(e => e.RecordId)
            .Select(g => g.OrderByDescending(r => r.LastUpdatedAt).FirstOrDefault())
            .ToListAsync();

        return records
            .Select(r => new HistoricalRecordResponseDto
            {
                Id = r.RecordId,
                Description = r.Description,
                Uri = r.Uri,
                Properties = r.Properties,
                OriginalId = r.OriginalId,
                Name = r.Name,
                ClassId = r.ClassId,
                ClassName = r.ClassName,
                DataSourceId = r.DataSourceId,
                ProjectId = r.ProjectId,
                LastUpdatedAt = r.LastUpdatedAt,
                LastUpdatedBy = r.LastUpdatedBy,
                IsArchived = r.IsArchived,
                Tags = r.Tags
            });
    }
}