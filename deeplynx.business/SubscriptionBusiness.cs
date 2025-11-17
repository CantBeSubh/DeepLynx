using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace deeplynx.business;

public class SubscriptionBusiness : ISubscriptionBusiness
{
    private readonly DeeplynxContext _context;
    private readonly ICacheBusiness _cacheBusiness;

    public SubscriptionBusiness(DeeplynxContext context, ICacheBusiness cacheBusiness)
    {
        _context = context;
        _cacheBusiness = cacheBusiness;
    }

    /// <summary>
    /// Retrieves all subscriptions with a particular userId and projectId
    /// </summary>
    /// <param name="userId">The ID of the user to which the subscription belongs</param>
    /// <param name="projectId">The ID of the project to which the subscription belongs</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived classes from the result</param>
    /// <returns>A list of subscriptions</returns>
    /// TODO: CACHE ALL SUBSCRIPTIONS TO MAKE THE NOTIFICATION SERVICE MORE EFFICIENT
    public async Task<List<SubscriptionResponseDto>> GetAllSubscriptions(long userId, long organizationId, bool hideArchived,
        long? projectId)
    {
        await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId);
        
        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
        }

        var query = _context.Subscriptions
            .Where(s => s.UserId == userId);

        // Filter by scope
        if (projectId.HasValue)
        {
            query = query.Where(s => s.ProjectId == projectId.Value);
        }
        else
        {
            query = query.Where(s => s.OrganizationId == organizationId);
        }

        // Filter archived in database
        if (hideArchived)
        {
            query = query.Where(s => !s.IsArchived);
        }

        var subscriptions = await query.ToListAsync();

        return subscriptions.Select(s => new SubscriptionResponseDto
        {
            Id = s.Id,
            UserId = s.UserId,
            OrganizationId = s.OrganizationId,
            ProjectId = s.ProjectId,
            ActionId = s.ActionId,
            Operation = s.Operation,
            DataSourceId = s.DataSourceId,
            EntityType = s.EntityType,
            EntityId = s.EntityId,
            LastUpdatedAt = s.LastUpdatedAt,
            LastUpdatedBy = s.LastUpdatedBy,
            IsArchived = s.IsArchived
        }).ToList();
    }

    /// <summary>
    /// Retrieves a single subscription with a particular userId, projectId, and subscriptionId
    /// </summary>
    /// <param name="userId">The ID of the user to which the subscription belongs</param>
    /// <param name="projectId">The ID of the project to which the subscription belongs</param>
    /// <param name="subscriptionId">The ID of the subscription</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived classes from the result</param>
    /// <returns>A list of subscriptions</returns>
    /// <exception cref="KeyNotFoundException">Returned if subscription is not found or is archived</exception>
    public async Task<SubscriptionResponseDto> GetSubscription(long userId, long subscriptionId, bool hideArchived,
        long organizationId, long? projectId)
    {
        // Validate existence of the scope
        await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId);
        
        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
        }

        var query = _context.Subscriptions
            .Where(s => s.Id == subscriptionId && s.UserId == userId);

        if (hideArchived)
        {
            query = query.Where(s => !s.IsArchived);
        }

        var subscription = await query.SingleOrDefaultAsync();

        if (subscription == null)
        {
            throw new KeyNotFoundException($"Subscription with id {subscriptionId} not found");
        }

        // Verify the subscription belongs to the specified scope
        if (projectId.HasValue && subscription.ProjectId != projectId.Value)
        {
            throw new UnauthorizedAccessException(
                $"Subscription {subscriptionId} does not belong to project {projectId.Value}");
        }

        if (subscription.OrganizationId != organizationId)
        {
            throw new UnauthorizedAccessException(
                $"Subscription {subscriptionId} does not belong to organization {organizationId}");
        }

        return new SubscriptionResponseDto
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            OrganizationId = subscription.OrganizationId,
            ProjectId = subscription.ProjectId,
            ActionId = subscription.ActionId,
            Operation = subscription.Operation,
            DataSourceId = subscription.DataSourceId,
            EntityType = subscription.EntityType,
            EntityId = subscription.EntityId,
            LastUpdatedAt = subscription.LastUpdatedAt,
            LastUpdatedBy = subscription.LastUpdatedBy,
            IsArchived = subscription.IsArchived
        };
    }

    /// <summary>
    /// Bulk creates subscriptions that are within the same scope (organization or project)
    /// </summary>
    /// <param name="userId">The ID of the user to which the subscriptions belong</param>
    /// <param name="organizationId">The ID of the organization to which the subscriptions belong (XOR with projectId)</param>
    /// <param name="projectId">The ID of the project to which the subscriptions belong (XOR with organizationId)</param>
    /// <returns>A list of created subscriptions</returns>
    public async Task<List<SubscriptionResponseDto>> BulkCreateSubscriptions(long userId, long organizationId,
        long? projectId,
        List<CreateSubscriptionRequestDto> dtos)
    {
        await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId);
        
        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
        }

        foreach (var dto in dtos)
        {
            ValidationHelper.ValidateModel(dto);

            // Validate the Operation property if it is not null
            if (dto.Operation != null)
            {
                ValidationHelper.ValidateTypes(dto.Operation, "Operation");
            }
        }

        // Determine which unique index applies
        var conflictColumns = projectId.HasValue
            ? "user_id, action_id, operation, project_id, data_source_id, entity_type, entity_id"
            : "user_id, action_id, operation, organization_id, data_source_id, entity_type, entity_id";

        var sql = $@"
        INSERT INTO deeplynx.subscriptions (user_id, action_id, operation, organization_id, project_id, data_source_id, entity_type, entity_id, last_updated_at, is_archived)
        VALUES {{0}}
        ON CONFLICT ({conflictColumns}) DO NOTHING
        RETURNING id, user_id, organization_id, project_id, action_id, operation, data_source_id, entity_type, entity_id, last_updated_at, last_updated_by, is_archived;
        ";

        var parameters = new List<NpgsqlParameter>
        {
            new("@now", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)),
        };

        parameters.AddRange(dtos.SelectMany((dto, i) => new[]
        {
            new NpgsqlParameter($"@p{i}_userId", userId),
            new NpgsqlParameter($"@p{i}_actionId", dto.ActionId),
            new NpgsqlParameter($"@p{i}_operation", (object?)dto.Operation ?? DBNull.Value),
            new NpgsqlParameter($"@p{i}_organizationId", (object?)organizationId ?? DBNull.Value),
            new NpgsqlParameter($"@p{i}_projectId", (object?)projectId ?? DBNull.Value),
            new NpgsqlParameter($"@p{i}_dataSourceId", (object?)dto.DataSourceId ?? DBNull.Value),
            new NpgsqlParameter($"@p{i}_entityType", (object?)dto.EntityType ?? DBNull.Value),
            new NpgsqlParameter($"@p{i}_entityId", (object?)dto.EntityId ?? DBNull.Value)
        }));

        var valueTuples = string.Join(", ", dtos.Select((dto, i) =>
            $"(@p{i}_userId, @p{i}_actionId, @p{i}_operation, @p{i}_organizationId, @p{i}_projectId, @p{i}_dataSourceId, @p{i}_entityType, @p{i}_entityId, @now, false)"));

        sql = string.Format(sql, valueTuples);

        var result = await _context.Subscriptions
            .FromSqlRaw(sql, parameters.ToArray())
            .ToListAsync();

        return result.Select(s => new SubscriptionResponseDto
        {
            Id = s.Id,
            UserId = s.UserId,
            ProjectId = s.ProjectId,
            OrganizationId = s.OrganizationId,
            ActionId = s.ActionId,
            Operation = s.Operation,
            DataSourceId = s.DataSourceId,
            EntityType = s.EntityType,
            EntityId = s.EntityId,
            LastUpdatedAt = s.LastUpdatedAt,
            LastUpdatedBy = s.LastUpdatedBy,
            IsArchived = s.IsArchived
        }).ToList();
    }

    /// <summary>
    /// Bulk updates subscriptions
    /// </summary>
    /// <param name="userId">The ID of the user to which the subscriptions belong</param>
    /// <param name="organizationId">The ID of the organization to which the subscriptions belong (XOR with projectId)</param>
    /// <param name="projectId">The ID of the project to which the subscriptions belong (XOR with organizationId)</param>
    /// <returns>A list of updated subscriptions</returns>
    public async Task<List<SubscriptionResponseDto>> BulkUpdateSubscriptions(
        long userId,
        long organizationId,
        long? projectId,
        List<UpdateSubscriptionRequestDto> dtos)
    {
        await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId);
        
        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
        }

        foreach (var dto in dtos)
        {
            ValidationHelper.ValidateModel(dto);

            if (dto.Operation != null)
            {
                ValidationHelper.ValidateTypes(dto.Operation, "Operation");
            }

            if ((dto.EntityType != null && dto.EntityId != null) || (dto.EntityType == null && dto.EntityId == null))
            {
                if (dto.EntityType != null)
                {
                    ValidationHelper.ValidateTypes(dto.EntityType, "EntityType");
                }
            }
            else
            {
                throw new ArgumentException("EntityType and EntityId must either both be null or both have values.");
            }
        }

        string scopeCondition = projectId.HasValue
            ? "AND subs.project_id = @scopeId"
            : "AND subs.organization_id = @scopeId";

        var sql = $@"
        UPDATE deeplynx.subscriptions AS subs
        SET 
            action_id = data.action_id,
            operation = data.operation,
            data_source_id = data.data_source_id,
            entity_type = data.entity_type,
            entity_id = data.entity_id,
            last_updated_at = @now
        FROM (VALUES {{0}}) AS data (id, action_id, operation, data_source_id, entity_type, entity_id)
        WHERE 
            subs.id = data.id 
            AND subs.user_id = @userId 
            {scopeCondition}
        RETURNING subs.id, subs.user_id, subs.organization_id, subs.project_id, subs.action_id, subs.operation, 
            subs.data_source_id, subs.entity_type, subs.entity_id, 
            subs.last_updated_at, subs.is_archived, subs.last_updated_by;
        ";

        var parameters = new List<NpgsqlParameter>
        {
            new("@userId", userId),
            new("@scopeId", projectId ?? organizationId),
            new("@now", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified))
        };

        parameters.AddRange(dtos.SelectMany((dto, i) => new[]
        {
            new NpgsqlParameter($"@p{i}_id", dto.Id),
            new NpgsqlParameter($"@p{i}_actionId", (object?)dto.ActionId ?? DBNull.Value),
            new NpgsqlParameter($"@p{i}_operation", (object?)dto.Operation ?? DBNull.Value),
            new NpgsqlParameter($"@p{i}_dataSourceId", (object?)dto.DataSourceId ?? DBNull.Value),
            new NpgsqlParameter($"@p{i}_entityType", (object?)dto.EntityType ?? DBNull.Value),
            new NpgsqlParameter($"@p{i}_entityId", (object?)dto.EntityId ?? DBNull.Value)
        }));

        var valueTuples = string.Join(", ", dtos.Select((dto, i) =>
            $"(@p{i}_id, @p{i}_actionId, @p{i}_operation, @p{i}_dataSourceId, @p{i}_entityType, @p{i}_entityId)"));

        sql = string.Format(sql, valueTuples);

        var updatedSubscriptions = await _context.Subscriptions
            .FromSqlRaw(sql, parameters.ToArray())
            .ToListAsync();

        var response = updatedSubscriptions.Select(s => new SubscriptionResponseDto
        {
            Id = s.Id,
            UserId = s.UserId,
            ProjectId = s.ProjectId,
            OrganizationId = s.OrganizationId,
            ActionId = s.ActionId,
            Operation = s.Operation,
            DataSourceId = s.DataSourceId,
            EntityType = s.EntityType,
            EntityId = s.EntityId,
            LastUpdatedAt = s.LastUpdatedAt,
            LastUpdatedBy = s.LastUpdatedBy,
            IsArchived = s.IsArchived
        }).ToList();
        
        if (response.Count != dtos.Count)
        {
            throw new InvalidOperationException(
                $"Some subscriptions were not updated. This may be because they do not exist, " +
                $"do not belong to user {userId}, or do not belong to the specified " +
                $"{(projectId.HasValue ? "project" : "organization")}.");
        }
        
        return response;
    }

    /// <summary>
    /// Bulk deletes subscriptions
    /// </summary>
    /// <param name="userId">The ID of the user to which the subscriptions belong</param>
    /// <param name="organizationId">The ID of the organization to which the subscriptions belong (XOR with projectId)</param>
    /// <param name="projectId">The ID of the project to which the subscriptions belong (XOR with organizationId)</param>
    /// <param name="subscriptionIds">The ID of the subscriptions to be deleted</param>
    /// <returns>True if successful</returns>
    public async Task<bool> BulkDeleteSubscriptions(long userId, long organizationId, long? projectId,
        List<long> subscriptionIds)
    {
        // Ensure exactly one scope is provided
        await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId);
        
        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
        }

        // Build the WHERE clause based on scope
        string scopeCondition = projectId.HasValue
            ? "AND project_id = @scopeId"
            : "AND organization_id = @scopeId";

        var sql = $@"
        DELETE FROM deeplynx.subscriptions
        WHERE 
            user_id = @userId 
            {scopeCondition}
            AND id = ANY(@subscriptionIds);
    ";

        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@userId", userId),
            new NpgsqlParameter("@scopeId", projectId ?? organizationId),
            new NpgsqlParameter("@subscriptionIds", subscriptionIds.ToArray())
        };

        var result = await _context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());

        if (result != subscriptionIds.Count)
        {
            throw new InvalidOperationException("Some subscriptions were not deleted because they do not exist.");
        }

        return true;
    }

    /// <summary>
    /// Bulk archives subscriptions
    /// </summary>
    /// <param name="userId">The ID of the user to which the subscriptions belong</param>
    /// <param name="organizationId">The ID of the organization to which the subscriptions belong (XOR with projectId)</param>
    /// <param name="projectId">The ID of the project to which the subscriptions belong (XOR with organizationId)</param>
    /// <param name="subscriptionIds">The ID of the subscriptions to be archived</param>
    /// <returns>True if successful</returns>
    public async Task<bool> BulkArchiveSubscriptions(long userId, long organizationId, long? projectId,
        List<long> subscriptionIds)
    {
        // Ensure exactly one scope is provided
        await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId);
        
        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
        }

        // Build the WHERE clause based on scope
        string scopeCondition = projectId.HasValue
            ? "AND project_id = @scopeId"
            : "AND organization_id = @scopeId";

        var sql = $@"
        UPDATE deeplynx.subscriptions
        SET is_archived = true, last_updated_at = @now
        WHERE 
            user_id = @userId 
            {scopeCondition}
            AND id = ANY(@subscriptionIds);
    ";

        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@userId", userId),
            new NpgsqlParameter("@scopeId", projectId ?? organizationId),
            new NpgsqlParameter("@subscriptionIds", subscriptionIds.ToArray()),
            new NpgsqlParameter("@now", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified))
        };

        var result = await _context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());

        if (result != subscriptionIds.Count)
        {
            throw new InvalidOperationException("Some subscriptions were not archived because they do not exist.");
        }

        return true;
    }

    /// <summary>
    /// Bulk unarchives subscriptions
    /// </summary>
    /// <param name="userId">The ID of the user to which the subscriptions belong</param>
    /// <param name="organizationId">The ID of the organization to which the subscriptions belong (XOR with projectId)</param>
    /// <param name="projectId">The ID of the project to which the subscriptions belong (XOR with organizationId)</param>
    /// <param name="subscriptionIds">The ID of the subscriptions to be unarchived</param>
    /// <returns>True if successful</returns>
    public async Task<bool> BulkUnarchiveSubscriptions(long userId, long organizationId, long? projectId,
        List<long> subscriptionIds)
    {
        await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId);
        
        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
        }
        
        // Build the WHERE clause based on scope
        string scopeCondition = projectId.HasValue
            ? "AND project_id = @scopeId"
            : "AND organization_id = @scopeId";

        var sql = $@"
        UPDATE deeplynx.subscriptions
        SET is_archived = false, last_updated_at = @now
        WHERE 
            user_id = @userId 
            {scopeCondition}
            AND id = ANY(@subscriptionIds);
    ";

        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@userId", userId),
            new NpgsqlParameter("@scopeId", projectId ?? organizationId),
            new NpgsqlParameter("@subscriptionIds", subscriptionIds.ToArray()),
            new NpgsqlParameter("@now", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified))
        };

        var result = await _context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());

        if (result != subscriptionIds.Count)
        {
            throw new InvalidOperationException("Some subscriptions were not unarchived because they do not exist.");
        }

        return true;
    }
}