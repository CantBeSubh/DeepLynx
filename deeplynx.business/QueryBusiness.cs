using System.Linq.Expressions;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.RegularExpressions;
using deeplynx.datalayer.Models;
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
    public async Task<IEnumerable<HistoricalRecordResponseDto>> Search(string userQuery)
    {
        if (string.IsNullOrWhiteSpace(userQuery))
            throw new Exception("Search query is required.");
        
        var query = ParseToQuery(userQuery);
        
        // full text search query for all text properties of historical records table 
        var sql = @"
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
               coalesce(last_updated_by, '') || ' ' ||
                coalesce(properties::text, '') || ' ' ||
                coalesce(tags::text, '')
            ) @@ to_tsquery('english', @query);
        ";

        var param = new NpgsqlParameter("query", query);

        var results = await _context.HistoricalRecords
            .FromSqlRaw(sql, param)
            .ToListAsync();
        
        return results
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
                LastUpdatedBy = r.LastUpdatedBy,
                IsArchived = r.IsArchived,
                Description = r.Description, 
                LastUpdatedAt = r.LastUpdatedAt
            });
    }
    
    private string ParseToQuery(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Operators to translate
        var operators = new HashSet<string> { "AND", "OR" };

        // Tokenize input by whitespace
        var tokens = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<string>();

        foreach (var token in tokens)
        {
            string upper = token.ToUpperInvariant();

            if (operators.Contains(upper))
            {
                switch (upper)
                {
                    case "AND": result.Add("&"); break;
                    case "OR": result.Add("|"); break;
                }
            }
            else
            {
                // Add :* for partial matching
                var cleaned = Regex.Replace(token, @"[^\w]", "");
                if (!string.IsNullOrWhiteSpace(cleaned))
                    result.Add($"{cleaned}:*");
            }
        }

        return string.Join(" ", result);
    }

}
