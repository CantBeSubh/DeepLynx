using Microsoft.EntityFrameworkCore;
using deeplynx.models;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using System.Text.Json;

namespace deeplynx.business;

public class SensitivityLabelBusiness : ISensitivityLabelBusiness
{
    private readonly DeeplynxContext _context;
    private readonly ICacheBusiness _cacheBusiness;
    private readonly IEventBusiness _eventBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="SensitivityLabelBusiness"/> class.
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
    /// Get all sensitivity labels for a given project and/or organization
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
            labelQuery = labelQuery.Where(x => x.ProjectId == projectId.Value);
        }

        if (organizationId.HasValue)
        {
            await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId.Value, hideArchived);
            labelQuery = labelQuery.Where(x => x.OrganizationId == organizationId.Value);
        }
        
        if (hideArchived)
            labelQuery = labelQuery.Where(l => !l.IsArchived);
        
        return await labelQuery.Select(l => new SensitivityLabelResponseDto()
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                LastUpdatedAt = l.LastUpdatedAt,
                LastUpdatedBy = l.LastUpdatedBy,
                ProjectId = l.ProjectId,
                OrganizationId = l.OrganizationId,
            })
            .ToListAsync();
    }

    /// <summary>
    /// Get a sensitivity label by ID
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
            OrganizationId = label.OrganizationId,
        };
    }

    /// <summary>
    /// Create a new sensitivity label
    /// </summary>
    /// <param name="dto">Data Transfer Object containing new label information</param>
    /// <param name="projectId">ID of the project to which the label belongs</param>
    /// <param name="organizationId">ID of the organization to which the label belongs</param>
    /// <returns>The newly created label</returns>
    /// <exception cref="ArgumentException">Returned if project/org both supplied or no project/org supplied</exception>
    public async Task<SensitivityLabelResponseDto> CreateSensitivityLabel(
        CreateSensitivityLabelRequestDto dto, long? projectId, long? organizationId)
    {
        // ensure one and only one of projectID or organizationID is supplied
        if (!projectId.HasValue && !organizationId.HasValue)
            throw new ArgumentException("One of Project ID or Organization ID must be provided");
        if (projectId.HasValue && organizationId.HasValue)
            throw new ArgumentException("Please provide only one of Project ID or Organization ID, not both");

        if (projectId.HasValue)
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
        if (organizationId.HasValue)
            await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId.Value);
        
        ValidationHelper.ValidateModel(dto);
        var label = new SensitivityLabel
        {
            Name = dto.Name,
            Description = dto.Description,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null, // TODO: implement user ID here when JWT tokens are ready,
            ProjectId = projectId.Value,
            OrganizationId = organizationId.Value,
        };
        
        _context.SensitivityLabels.Add(label);
        await _context.SaveChangesAsync();
        
        // Log create SensitivityLabel event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = organizationId.Value,
            ProjectId = projectId.Value,
            Operation = "create",
            EntityType = "sensitivity_label",
            EntityId = label.Id,
            Properties = JsonSerializer.Serialize(new { label.Name }),
            LastUpdatedBy = "" // TODO: add username when JWTs are implemented
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
            OrganizationId = label.OrganizationId,
        };
    }

    /// <summary>
    /// Update sensitivity label information
    /// </summary>
    /// <param name="labelId">ID of the label to be updated</param>
    /// <param name="dto">Data Transfer Object containing new label information</param>
    /// <returns>The newly updated label</returns>
    /// <exception cref="KeyNotFoundException">Returned if label not found</exception>
    public async Task<SensitivityLabelResponseDto> UpdateSensitivityLabel(
        long labelId, UpdateSensitivityLabelRequestDto dto)
    {
        var label = await _context.SensitivityLabels.FindAsync(labelId);
        if (label == null || label.IsArchived)
            throw new KeyNotFoundException($"Sensitivity label with id {labelId} not found");
        
        label.Name = dto.Name ?? label.Name;
        label.Description = dto.Description ?? label.Description;
        label.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        label.LastUpdatedBy = null;  // TODO: implement user ID here when JWT tokens are ready
        
        _context.SensitivityLabels.Update(label);
        await _context.SaveChangesAsync();
        
        // Log update SensitivityLabel event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = label.OrganizationId,
            ProjectId = label.ProjectId,
            Operation = "update",
            EntityType = "sensitivity_label",
            EntityId = label.Id,
            Properties = JsonSerializer.Serialize(new { label.Name }),
            LastUpdatedBy = "" // TODO: add username when JWTs are implemented
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
            OrganizationId = label.OrganizationId,
        };
    }

    /// <summary>
    /// Archive a sensitivity label by ID.
    /// </summary>
    /// <param name="labelId">ID of label to archive</param>
    /// <returns>Boolean true if executed successfully</returns>
    /// <exception cref="KeyNotFoundException">Returned if label not found or is already archived</exception>
    public async Task<bool> ArchiveSensitivityLabel(long labelId)
    {
        var label = await _context.SensitivityLabels.FindAsync(labelId);
        if (label == null || label.IsArchived)
            throw new KeyNotFoundException($"Sensitivity label with id {labelId} not found or is archived");

        label.IsArchived = true;
        label.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        label.LastUpdatedBy = null; // TODO: add username when JWTs are implemented
        _context.SensitivityLabels.Update(label);
        await _context.SaveChangesAsync();
        
        // Log archive SensitivityLabel event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = label.OrganizationId,
            ProjectId = label.ProjectId,
            Operation = "archive",
            EntityType = "sensitivity_label",
            EntityId = label.Id,
            Properties = JsonSerializer.Serialize(new { label.Name }),
            LastUpdatedBy = "" // TODO: add username when JWTs are implemented
        });
        
        return true;
    }
    
    /// <summary>
    /// Unarchive a sensitivity label by ID
    /// </summary>
    /// <param name="labelId">ID of label to unarchive</param>
    /// <returns>Boolean true if executed successfully</returns>
    /// <exception cref="KeyNotFoundException">Returned if label not found or is not archived</exception>
    public async Task<bool> UnarchiveSensitivityLabel(long labelId)
    {
        var label = await _context.SensitivityLabels.FindAsync(labelId);
        if (label == null || !label.IsArchived)
            throw new KeyNotFoundException($"Sensitivity label with id {labelId} not found or is not archived");

        label.IsArchived = false;
        label.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        label.LastUpdatedBy = null; // TODO: add username when JWTs are implemented
        _context.SensitivityLabels.Update(label);
        await _context.SaveChangesAsync();
        
        // Log unarchive SensitivityLabel event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = label.OrganizationId,
            ProjectId = label.ProjectId,
            Operation = "unarchive",
            EntityType = "sensitivity_label",
            EntityId = label.Id,
            Properties = JsonSerializer.Serialize(new { label.Name }),
            LastUpdatedBy = "" // TODO: add username when JWTs are implemented
        });
        
        return true;
    }
    
    /// <summary>
    /// Delete a sensitivity label by ID
    /// </summary>
    /// <param name="labelId">ID of label to delete</param>
    /// <returns>Boolean true if executed successfully</returns>
    /// <exception cref="KeyNotFoundException">Returned if label not found</exception>
    public async Task<bool> DeleteSensitivityLabel(long labelId)
    {
        var label = await _context.SensitivityLabels.FindAsync(labelId);
        if (label == null || label.IsArchived)
            throw new KeyNotFoundException($"Sensitivity label with id {labelId} not found or is archived");

        _context.SensitivityLabels.Remove(label);
        await _context.SaveChangesAsync();
        
        // Log delete SensitivityLabel event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = label.OrganizationId,
            ProjectId = label.ProjectId,
            Operation = "delete",
            EntityType = "sensitivity_label",
            EntityId = label.Id,
            Properties = JsonSerializer.Serialize(new { label.Name }),
            LastUpdatedBy = "" // TODO: add username when JWTs are implemented
        });
        
        return true;
    }
}