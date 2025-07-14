using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace deeplynx.business;

public class Metadatabusiness : IMetadataBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IRecordMappingBusiness _recordMappingBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context to be used for tag operations.</param>
    public Metadatabusiness(DeeplynxContext context, IRecordMappingBusiness recordMappingBusiness)
    {
        _context = context;
        _recordMappingBusiness = recordMappingBusiness;
    }

    /// <summary>
    /// Asynchronously creates a new tag for a specified project.
    /// Note: Will error out with foreign key constraint violation if project is not found.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagRequestDto">The tag request data transfer object containing tag details.</param>
    /// <returns>The created tag response DTO with saved details.</returns>
    public async Task<MetadataResponseDto> CreateMetadata(long projectId, MetadataRequestDto metadataRequestDto)
    {
        DoesProjectExist(projectId);
        if (metadataRequestDto == null)
            throw new ArgumentNullException(nameof(metadataRequestDto));
        
        // Validate 'Name' field
        if (string.IsNullOrWhiteSpace(metadataRequestDto.Name))
        {
            throw new ValidationException("Name is required and cannot be empty or whitespace");
        }

        return new MetadataResponseDto() // Return validated response DTO back to user.
        {
            Id = metadataRequestDto.Id,
            Name = metadataRequestDto.Name,
            ProjectId = metadataRequestDto.ProjectId,
            CreatedBy = metadataRequestDto.CreatedBy,
            CreatedAt = metadataRequestDto.CreatedAt
        };
    }
    
    /// <summary>
    /// Determine if project exists
    /// </summary>
    /// <param name="projectId">The ID of the project we are searching for</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived projects from the result (Default true)</param>
    /// <returns>Throws error if project does not exist</returns>
    private void DoesProjectExist(long projectId, bool hideArchived = true)
    {
        var project = hideArchived ? _context.Projects.Any(p => p.Id == projectId && p.ArchivedAt == null) 
            : _context.Projects.Any(p => p.Id == projectId);
        if (!project)
        {
            throw new KeyNotFoundException($"Project with id {projectId} not found");
        }
    }
}