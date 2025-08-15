using System.Linq.Expressions;
using System.Text.RegularExpressions;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;

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
    /// Advanced query builder
    /// </summary>
    /// <param name="queryRequests">Array of query component dtos</param>
    /// <returns>A list of historical record response dtos that match provided filters</returns>
  public IEnumerable<HistoricalRecordResponseDto> BuildQuery(string initialQuery, CustomQueryRequestDto[] queryRequests)
{
    IQueryable<HistoricalRecord> histRec;
    
    // If we have both filters and full-text search, combine them in a single SQL query
    if (queryRequests.Any() && !string.IsNullOrWhiteSpace(initialQuery))
    {
        histRec = BuildCombinedQuery(initialQuery, queryRequests);
    }
    // If only filters, use Expression trees
    else if (queryRequests.Any())
    {
        histRec = BuildFilterQuery(queryRequests);
    }
    // If only full-text search, use raw SQL
    else if (!string.IsNullOrWhiteSpace(initialQuery))
    {
        histRec = BuildFullTextQuery(initialQuery);
    }
    // No filters
    else
    {
        histRec = _context.HistoricalRecords.AsQueryable();
    }

    // Execute & shape
    return histRec
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
            CreatedBy = r.CreatedBy,
            CreatedAt = r.CreatedAt,
            ModifiedBy = r.ModifiedBy,
            ModifiedAt = r.ModifiedAt,
            ArchivedAt = r.ArchivedAt,
            LastUpdatedAt = r.LastUpdatedAt,
            Description = r.Description
        })
        .ToList();
}

private IQueryable<HistoricalRecord> BuildCombinedQuery(string initialQuery, CustomQueryRequestDto[] queryRequests)
{
    var tsQueryText = ParseToQuery(initialQuery);
    var whereConditions = new List<string>();
    var parameters = new List<NpgsqlParameter>();
    
    // Add parameter for full-text search
    parameters.Add(new NpgsqlParameter("query", tsQueryText));
    
    // Build WHERE conditions from queryRequests
    for (int i = 0; i < queryRequests.Length; i++)
    {
        var query = queryRequests[i];
        var paramName = $"param{i}";
        
        string condition;
        if (query.Operator.ToUpper() == "LIKE")
        {
            condition = $"{ConvertPropertyToColumnName(query.Filter)} ILIKE @{paramName}";
            parameters.Add(new NpgsqlParameter(paramName, $"%{query.Value}%"));
        }
        else if (query.Operator == "=")
        {
            condition = $"{ConvertPropertyToColumnName(query.Filter)} = @{paramName}";
            parameters.Add(new NpgsqlParameter(paramName, ConvertValueForSql(query.Value, query.Filter)));
        }
        // Add more operators as needed
        else
        {
            condition = $"{ConvertPropertyToColumnName(query.Filter)} {query.Operator} @{paramName}";
            parameters.Add(new NpgsqlParameter(paramName, ConvertValueForSql(query.Value, query.Filter)));
        }
        
        whereConditions.Add(condition);
    }
    
    var whereClause = string.Join(" AND ", whereConditions);
    
    var sql = $@"
        SELECT *
        FROM deeplynx.historical_records
        WHERE ({whereClause})
        AND to_tsvector('english',
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
        ) @@ to_tsquery('english', @query)";

    return _context.HistoricalRecords.FromSqlRaw(sql, parameters.ToArray());
}

private IQueryable<HistoricalRecord> BuildFilterQuery(CustomQueryRequestDto[] queryRequests)
{
    var histRec = _context.HistoricalRecords.AsQueryable();
    var parameter = Expression.Parameter(typeof(HistoricalRecord), "historicalRecord");
    Expression combinedExpression = null;

    foreach (var query in queryRequests)
    {
        var property = Expression.Property(parameter, query.Filter);
        var targetType = Nullable.GetUnderlyingType(property.Type) ?? property.Type;

        object? convertedValue = null;
        if (!string.IsNullOrWhiteSpace(query.Value))
        {
            try
            {
                convertedValue = Convert.ChangeType(query.Value, targetType);
            }
            catch (Exception)
            {
                convertedValue = query.Value;
            }
        }

        Expression value;
        if (convertedValue == null)
        {
            value = Expression.Constant(null, property.Type);
        }
        else
        {
            if (property.Type != targetType && Nullable.GetUnderlyingType(property.Type) != null)
            {
                value = Expression.Constant(convertedValue, property.Type);
            }
            else
            {
                value = Expression.Constant(convertedValue, property.Type);
            }
        }
        
        Expression comparison = null;
        if (query.Operator == "LIKE")
        {
            // Your existing LIKE logic here
            var efFunctionsProp = typeof(EF).GetProperty(nameof(EF.Functions))!;
            var efFunctionsExpr = Expression.Property(null, efFunctionsProp);
            var patternExpr = Expression.Constant($"%{query.Value}%");
            var mi = typeof(NpgsqlDbFunctionsExtensions).GetMethod(
                nameof(NpgsqlDbFunctionsExtensions.ILike),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) }
            )!;
            comparison = Expression.Call(mi, efFunctionsExpr, property, patternExpr);
        }
        else
        {
            comparison = CreateComparisonExpression(property, value, query.Operator);
        }
     
        combinedExpression = CombineExpressions(combinedExpression, comparison, query.Connector);
    }

    if (combinedExpression != null)
    {
        var lambda = Expression.Lambda<Func<HistoricalRecord, bool>>(combinedExpression, parameter);
        histRec = histRec.Where(lambda);
    }
    
    return histRec;
}

private IQueryable<HistoricalRecord> BuildFullTextQuery(string initialQuery)
{
    var tsQueryText = ParseToQuery(initialQuery);
    
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
            coalesce(created_by, '') || ' ' ||
            coalesce(modified_by, '') || ' ' ||
            coalesce(properties::text, '') || ' ' ||
            coalesce(tags::text, '')
        ) @@ to_tsquery('english', @query)";

    var param = new NpgsqlParameter("query", tsQueryText);
    return _context.HistoricalRecords.FromSqlRaw(sql, param);
}

// Helper method to convert C# property names to database column names
private string ConvertPropertyToColumnName(string propertyName)
{
    // Convert PascalCase to snake_case for PostgreSQL
    return string.Concat(propertyName.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToLower();
}

// Helper method to convert values for SQL parameters
private object ConvertValueForSql(string value, string propertyName)
{
    // Add logic here to convert based on the property type
    // For now, just return the string value
    return value;
}
    private Expression CreateComparisonExpression(MemberExpression property, Expression value, string operatorType)
    {
        return operatorType switch
        {
            //TODO: include LIKE operator, IN  for arrays
            //TODO: Logic for operator and value type 
            "=" => Expression.Equal(property, value),
            ">" => Expression.GreaterThan(property, value),
            "<" => Expression.LessThan(property, value),
            ">=" => Expression.GreaterThanOrEqual(property, value),
            "<=" => Expression.LessThanOrEqual(property, value),
            "!=" => Expression.NotEqual(property, value),
            _ => throw new NotSupportedException($"Operator {operatorType} is not supported")
        };
    }

    private Expression CombineExpressions(Expression existing, Expression newExpression, string connector)
    {
        //TODO: Fix initial connector being OR
        if (existing == null)
        {
            return newExpression;
        }

        return connector switch
        {
            "AND" => Expression.AndAlso(existing, newExpression),
            "OR" => Expression.OrElse(existing, newExpression),
            _ => throw new NotSupportedException($"Connector {connector} is not supported")
        };
    }
    
    
    
    /// <summary>
    /// Google-type search
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
                coalesce(created_by, '') || ' ' ||
                coalesce(modified_by, '') || ' ' ||
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
                CreatedBy = r.CreatedBy,
                CreatedAt = r.CreatedAt,
                ModifiedBy = r.ModifiedBy,
                ModifiedAt = r.ModifiedAt,
                ArchivedAt = r.ArchivedAt,
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
