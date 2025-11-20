using System.Text.Json;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class SensitivityLabelBusiness : ISensitivityLabelBusiness
{
    private readonly ICacheBusiness _cacheBusiness;
    private readonly DeeplynxContext _context;
    private readonly IEventBusiness _eventBusiness;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SensitivityLabelBusiness" /> class.
    /// </summary>
    /// <param name="context">The database context to be used for sensitivity label operations</param>
    /// <param name="cacheBusiness">Used to access cache operations</param>
    /// <param name="eventBusiness">Used for logging events during CRUD operations</param>
    public SensitivityLabelBusiness(DeeplynxContext context, ICacheBusiness cacheBusiness, IEventBusiness eventBusiness)
    {
        _context = context;
        _cacheBusiness = cacheBusiness;
        _eventBusiness = eventBusiness;
    }

    /// <summary>
    ///     Get all sensitivity labels for a given project and/or organization
    /// </summary>
    /// <param name="projectId">ID of the project across which to search</param>
    /// <param name="organizationId">ID of the organization across which to search</param>
    /// <param name="hideArchived">Flag indicating whether to search on archived labels</param>
    /// <returns>A list of labels</returns>
    public async Task<IEnumerable<SensitivityLabelResponseDto>> GetAllSensitivityLabels(
        long? projectId, long? organizationId, bool hideArchived = true)
    {
        var labelQuery = _context.SensitivityLabels.AsQueryable();

        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness, hideArchived);
            labelQuery = labelQuery.Where(x => x.ProjectId == projectId);
        }

        if (organizationId.HasValue)
        {
            await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId.Value, hideArchived);
            labelQuery = labelQuery.Where(x => x.OrganizationId == organizationId);
        }

        if (hideArchived)
            labelQuery = labelQuery.Where(l => !l.IsArchived);

        return await labelQuery.Select(l => new SensitivityLabelResponseDto
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                LastUpdatedAt = l.LastUpdatedAt,
                LastUpdatedBy = l.LastUpdatedBy,
                ProjectId = l.ProjectId,
                OrganizationId = l.OrganizationId,
                IsArchived = l.IsArchived
            })
            .ToListAsync();
    }

    /// <summary>
    ///     Get a sensitivity label by ID
    /// </summary>
    /// <param name="labelId">ID of the label to retrieve</param>
    /// <param name="hideArchived">Flag indicating whether to search archived labels</param>
    /// <returns>The requested label</returns>
    /// <exception cref="KeyNotFoundException">Thrown if label not found</exception>
    public async Task<SensitivityLabelResponseDto> GetSensitivityLabel(long labelId, bool hideArchived = true)
    {
        var label = await _context.SensitivityLabels.FirstOrDefaultAsync(l => l.Id == labelId);

        if (label == null)
            throw new KeyNotFoundException($"Sensitivity label with id {labelId} not found");

        if (hideArchived && label.IsArchived)
            throw new KeyNotFoundException($"Sensitivity label with id {labelId} is archived");

        return new SensitivityLabelResponseDto
        {
            Id = label.Id,
            Name = label.Name,
            Description = label.Description,
            LastUpdatedAt = label.LastUpdatedAt,
            LastUpdatedBy = label.LastUpdatedBy,
            IsArchived = label.IsArchived,
            ProjectId = label.ProjectId,
            OrganizationId = label.OrganizationId
        };
    }

    /// <summary>
    ///     Update sensitivity label information
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="labelId">ID of the label to be updated</param>
    /// <param name="dto">Data Transfer Object containing new label information</param>
    /// <returns>The newly updated label</returns>
    /// <exception cref="KeyNotFoundException">Returned if label not found</exception>
    public async Task<SensitivityLabelResponseDto> UpdateSensitivityLabel(
        long currentUserId, long labelId, UpdateSensitivityLabelRequestDto dto)
    {
        var label = await _context.SensitivityLabels.FindAsync(labelId);
        if (label == null || label.IsArchived)
            throw new KeyNotFoundException($"Sensitivity label with id {labelId} not found");

        label.Name = dto.Name ?? label.Name;
        label.Description = dto.Description ?? label.Description;
        label.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        label.LastUpdatedBy = currentUserId;

        _context.SensitivityLabels.Update(label);
        await _context.SaveChangesAsync();

        // Log update SensitivityLabel event
        await _eventBusiness.CreateEvent(currentUserId, new CreateEventRequestDto
        {
            OrganizationId = label.OrganizationId,
            ProjectId = label.ProjectId,
            Operation = "update",
            EntityType = "sensitivity_label",
            EntityId = label.Id,
            EntityName = label.Name,
            Properties = JsonSerializer.Serialize(new { label.Name })
        });

        return new SensitivityLabelResponseDto
        {
            Id = label.Id,
            Name = label.Name,
            Description = label.Description,
            LastUpdatedAt = label.LastUpdatedAt,
            LastUpdatedBy = label.LastUpdatedBy,
            IsArchived = label.IsArchived,
            ProjectId = label.ProjectId,
            OrganizationId = label.OrganizationId
        };
    }

    /// <summary>
    ///     Archive a sensitivity label by ID.
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="labelId">ID of label to archive</param>
    /// <returns>Boolean true if executed successfully</returns>
    /// <exception cref="KeyNotFoundException">Returned if label not found or is already archived</exception>
    public async Task<bool> ArchiveSensitivityLabel(long currentUserId, long labelId)
    {
        var label = await _context.SensitivityLabels.FindAsync(labelId);
        if (label == null || label.IsArchived)
            throw new KeyNotFoundException($"Sensitivity label with id {labelId} not found or is archived");

        label.IsArchived = true;
        label.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        label.LastUpdatedBy = currentUserId;
        await _context.SaveChangesAsync();

        // Log archive SensitivityLabel event
        await _eventBusiness.CreateEvent(currentUserId, new CreateEventRequestDto
        {
            OrganizationId = label.OrganizationId,
            ProjectId = label.ProjectId,
            Operation = "archive",
            EntityType = "sensitivity_label",
            EntityId = label.Id,
            EntityName = label.Name,
            Properties = JsonSerializer.Serialize(new { label.Name })
        });

        return true;
    }

    /// <summary>
    ///     Unarchive a sensitivity label by ID
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="labelId">ID of label to unarchive</param>
    /// <returns>Boolean true if executed successfully</returns>
    /// <exception cref="KeyNotFoundException">Returned if label not found or is not archived</exception>
    public async Task<bool> UnarchiveSensitivityLabel(long currentUserId, long labelId)
    {
        var label = await _context.SensitivityLabels.FindAsync(labelId);
        if (label == null || !label.IsArchived)
            throw new KeyNotFoundException($"Sensitivity label with id {labelId} not found or is not archived");

        label.IsArchived = false;
        label.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        label.LastUpdatedBy = currentUserId;
        await _context.SaveChangesAsync();

        // Log unarchive SensitivityLabel event
        await _eventBusiness.CreateEvent(currentUserId, new CreateEventRequestDto
        {
            OrganizationId = label.OrganizationId,
            ProjectId = label.ProjectId,
            Operation = "unarchive",
            EntityType = "sensitivity_label",
            EntityId = label.Id,
            EntityName = label.Name,
            Properties = JsonSerializer.Serialize(new { label.Name })
        });

        return true;
    }

    /// <summary>
    ///     Delete a sensitivity label by ID
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="labelId">ID of label to delete</param>
    /// <returns>Boolean true if executed successfully</returns>
    /// <exception cref="KeyNotFoundException">Returned if label not found</exception>
    public async Task<bool> DeleteSensitivityLabel(long currentUserId, long labelId)
    {
        var label = await _context.SensitivityLabels.FindAsync(labelId);
        if (label == null || label.IsArchived)
            throw new KeyNotFoundException($"Sensitivity label with id {labelId} not found or is archived");

        _context.SensitivityLabels.Remove(label);
        await _context.SaveChangesAsync();

        // Log delete SensitivityLabel event
        await _eventBusiness.CreateEvent(currentUserId, new CreateEventRequestDto
        {
            OrganizationId = label.OrganizationId,
            ProjectId = label.ProjectId,
            Operation = "delete",
            EntityType = "sensitivity_label",
            EntityId = label.Id,
            EntityName = label.Name,
            Properties = JsonSerializer.Serialize(new { label.Name })
        });

        return true;
    }

    /// <summary>
    ///     Create a new sensitivity label
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="dto">Data Transfer Object containing new label information</param>
    /// <param name="projectId">ID of the project to which the label belongs</param>
    /// <param name="organizationId">ID of the organization to which the label belongs</param>
    /// <returns>The newly created label</returns>
    /// <exception cref="ArgumentException">Returned if project/org both supplied or no project/org supplied</exception>
    public async Task<SensitivityLabelResponseDto> CreateSensitivityLabel(
        long currentUserId, CreateSensitivityLabelRequestDto dto, long? projectId, long organizationId)
    {
        ValidationHelper.ValidateModel(dto);

        await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId);

        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);

            // We can also check here for proper project/org connection
            var project = await _context.Projects.FindAsync(projectId.Value);
            if (project?.OrganizationId != organizationId)
                throw new ArgumentException(
                    $"Project {projectId.Value} does not belong to organization {organizationId}");
        }

        var label = new SensitivityLabel
        {
            Name = dto.Name,
            Description = dto.Description,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = currentUserId,
            ProjectId = projectId,
            OrganizationId = organizationId
        };

        _context.SensitivityLabels.Add(label);
        await _context.SaveChangesAsync();

        // Log create SensitivityLabel event
        await _eventBusiness.CreateEvent(currentUserId, new CreateEventRequestDto
        {
            OrganizationId = organizationId,
            ProjectId = projectId,
            Operation = "create",
            EntityType = "sensitivity_label",
            EntityId = label.Id,
            EntityName = label.Name,
            Properties = JsonSerializer.Serialize(new { label.Name })
        });

        return new SensitivityLabelResponseDto
        {
            Id = label.Id,
            Name = label.Name,
            Description = label.Description,
            LastUpdatedAt = label.LastUpdatedAt,
            LastUpdatedBy = label.LastUpdatedBy,
            IsArchived = label.IsArchived,
            ProjectId = label.ProjectId,
            OrganizationId = label.OrganizationId
        };
    }
}