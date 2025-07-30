using System.Linq.Expressions;
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
    /// Filters record request
    /// </summary>
    /// <param name="filterRequest">Filter Request DTO</param>
    /// <returns>A list of record response dtos that match provided filters</returns>
    public async Task<IEnumerable<RecordResponseDto>> FilterRecords(string[] filterRequest)
    {
        var query = _context.HistoricalRecords.AsQueryable();

        // Check database for partial match, ignore case
        query = query.Where(c => filterRequest.Any(filter =>
            c.Name.ToLower().Contains(filter.ToLower()) ||
            c.Description.ToLower().Contains(filter.ToLower())));

        var records = await query.ToListAsync();

        return records
            .Select(r => new RecordResponseDto()
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
    
    /// <summary>
    /// Advanced query builder
    /// </summary>
    /// <param name="queryRequests">Array of query component dtos</param>
    /// <returns>A list of historical record response dtos that match provided filters</returns>
    public IEnumerable<HistoricalRecordResponseDto> BuildQuery(AdvancedQueryRequestDto[] queryRequests)
    {
        var histRec = _context.HistoricalRecords.AsQueryable();
        // Creates a parameter expression that represents a single instance of the HistoricalRecord class
        var parameter = Expression.Parameter(typeof(HistoricalRecord), "historicalRecord");
        Expression combinedExpression = null;

        foreach (var query in queryRequests)
        {
            // Creates an expression that represents accessing a property of the HistoricalRecord class
            var property = Expression.Property(parameter, query.Filter);
            // Converts the value from the DTO to the type of the property being filtered.
            var value = Expression.Constant(Convert.ChangeType(query.Value, property.Type));
            // Create the comparison operation (e.g., x.Name == "value")
            Expression comparison = CreateComparisonExpression(property, value, query.Operator);

            // Combine the expressions using the connector (AND/OR)
            combinedExpression = CombineExpressions(combinedExpression, comparison, query.Connector);
        }

        // Create the final lambda expression
        var lambda = Expression.Lambda<Func<HistoricalRecord, bool>>(combinedExpression, parameter);
        var records = histRec.Where(lambda).ToList();
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
                LastUpdatedAt = r.LastUpdatedAt
            });
    }

    private Expression CreateComparisonExpression(MemberExpression property, ConstantExpression value, string operatorType)
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

}
