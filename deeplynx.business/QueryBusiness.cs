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
    var histRec = _context.HistoricalRecords.AsQueryable();
    // Creates a parameter expression that represents a single instance of the HistoricalRecord class
    var parameter = Expression.Parameter(typeof(HistoricalRecord), "historicalRecord");
    Expression combinedExpression = null;

    foreach (var query in queryRequests)
    {
        // Creates an expression that represents accessing a property of the HistoricalRecord class
        var property = Expression.Property(parameter, query.Filter);
        
        // Get the target type (unwrap nullable if needed)
        var targetType = Nullable.GetUnderlyingType(property.Type) ?? property.Type;

        // Convert value to appropriate type
        object? convertedValue = null;
        if (!string.IsNullOrWhiteSpace(query.Value))
        {
            try
            {
                convertedValue = Convert.ChangeType(query.Value, targetType);
            }
            catch (Exception)
            {
                // If conversion fails, treat as string for LIKE operations
                convertedValue = query.Value;
            }
        }

        // Create the constant expression with the correct type
        Expression value;
        if (convertedValue == null)
        {
            // For null values, create a constant of the property's actual type
            value = Expression.Constant(null, property.Type);
        }
        else
        {
            // For non-null values, ensure the constant matches the property type
            if (property.Type != targetType && Nullable.GetUnderlyingType(property.Type) != null)
            {
                // This is a nullable type, so create the nullable value
                value = Expression.Constant(convertedValue, property.Type);
            }
            else
            {
                value = Expression.Constant(convertedValue, property.Type);
            }
        }
        
        // Create the comparison operation
        Expression comparison = null; 
        if (query.Operator == "LIKE")
        {
            // For LIKE operations, work with string representation
            Expression stringProperty;
            
            if (property.Type == typeof(string))
            {
                stringProperty = property;
            }
            else
            {
                // Convert non-string properties to string for LIKE operations
                // Handle nullable types by using null coalescing
                if (Nullable.GetUnderlyingType(property.Type) != null)
                {
                    // For nullable types, convert to string with null handling
                    var toStringMethod = typeof(object).GetMethod("ToString");
                    var convertToString = Expression.Call(
                        Expression.Convert(property, typeof(object)), 
                        toStringMethod
                    );
                    stringProperty = Expression.Coalesce(
                        Expression.Condition(
                            Expression.Equal(property, Expression.Constant(null, property.Type)),
                            Expression.Constant(""),
                            convertToString
                        ),
                        Expression.Constant("")
                    );
                }
                else
                {
                    // For non-nullable types
                    var toStringMethod = property.Type.GetMethod("ToString", Type.EmptyTypes);
                    stringProperty = Expression.Call(property, toStringMethod);
                }
            }

            // EF.Functions setup (your existing code works well)
            var efFunctionsProp = typeof(EF).GetProperty(nameof(EF.Functions))!;
            var efFunctionsExpr = Expression.Property(null, efFunctionsProp);

            // Wrap value in % for wildcard search
            var patternExpr = Expression.Constant($"%{query.Value}%");

            // Call EF.Functions.ILike for case-insensitive search
            var mi = typeof(NpgsqlDbFunctionsExtensions).GetMethod(
                nameof(NpgsqlDbFunctionsExtensions.ILike),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) }
            )!;

            comparison = Expression.Call(mi, efFunctionsExpr, stringProperty, patternExpr);
        }
        else
        {
            // For other operators, handle nullable comparisons properly
            if (convertedValue == null)
            {
                // For null comparisons, use IS NULL or IS NOT NULL
                if (query.Operator == "=" || query.Operator == "==")
                {
                    comparison = Expression.Equal(property, value);
                }
                else if (query.Operator == "!=" || query.Operator == "<>")
                {
                    comparison = Expression.NotEqual(property, value);
                }
                else
                {
                    // For other operators with null, this will typically be false
                    comparison = Expression.Constant(false);
                }
            }
            else
            {
                comparison = CreateComparisonExpression(property, value, query.Operator);
            }
        }
     
        // Combine the expressions using the connector (AND/OR)
        combinedExpression = CombineExpressions(combinedExpression, comparison, query.Connector);
    }

    // Create the final lambda expression
    var lambda = Expression.Lambda<Func<HistoricalRecord, bool>>(combinedExpression, parameter);
    histRec = histRec.Where(lambda);
    
    // 2) Full-text search AFTER filters to reduce amount of records queried with vector 
    if (!string.IsNullOrWhiteSpace(initialQuery))
    {
        // Build tsquery string once
        var tsQueryText = ParseToQuery(initialQuery);

        histRec = histRec.Where(r =>
            EF.Functions.ToTsVector("english",
                    (r.Name ?? "") + " " +
                    (r.Description ?? "") + " " +
                    (r.ClassName ?? "") + " " +
                    (r.Uri ?? "") + " " +
                    (r.OriginalId ?? "") + " " +
                    (r.DataSourceName ?? "") + " " +
                    (r.ProjectName ?? "") + " " +
                    (r.CreatedBy ?? "") + " " +
                    (r.ModifiedBy ?? "") + " "
                )
                .Matches(EF.Functions.WebSearchToTsQuery("english", tsQueryText)));
    }

    // 3) Execute & shape
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
