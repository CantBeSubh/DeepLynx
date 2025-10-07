using deeplynx.helpers;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers;

[Route("/projects/{projectId}/tags")]
[ApiController]
[NexusAuthorize]
public class TagController : ControllerBase
{
    private readonly ITagBusiness _tagBusiness;
    private readonly ILogger<TagController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagController"/> class.
    /// </summary>
    /// <param name="tagBusiness">The business logic interface for handling tag operations.</param>
    /// <param name="logger">Error/Info logging interface for database log table.</param>
    public TagController(ITagBusiness tagBusiness, ILogger<TagController> logger)
    {
        _tagBusiness = tagBusiness;
        _logger = logger;
    }

    /// <summary>
    /// Get all tags
    /// </summary>
    /// <param name="projectId">The ID of the project whose tags are to be retrieved.</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived tags from the result (Default true)</param>
    /// <returns>A list of tags belonging to the project.</returns>
    [HttpGet("GetAllTags", Name = "api_get_all_tags")]
    public async Task<ActionResult<IEnumerable<TagResponseDto>>> GetAllTags(
        long projectId, [FromQuery] bool hideArchived = true)
    {
        try
        {
            var tags = await _tagBusiness.GetAllTags(projectId,  hideArchived);
            return Ok(tags);
        }
        catch (Exception exception)
        {
            var message = $"An error occurred while listing all tags: {exception}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    /// Get a tag
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to retrieve.</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived tags from the result (Default true)</param>
    /// <returns>The tag with its details.</returns>
    [HttpGet("GetTag/{tagId}", Name = "api_get_a_tag")]
    public async Task<ActionResult<TagResponseDto>> GetTag(
        long projectId, 
        long tagId,
        [FromQuery] bool hideArchived = true)
    {
        try
        {
            var tag = await _tagBusiness.GetTag(projectId, tagId,  hideArchived);
            return Ok(tag);
        }
        catch (Exception exception)
        {
            var message = $"An error occurred while retrieving tag {tagId}: {exception}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }


    /// <summary>
    /// Creates a tag
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagRequestDto">The tag data transfer object containing tag details.</param>
    /// <returns>The created tag with its details.</returns>
    [HttpPost("CreateTag", Name = "api_create_a_tag")]
    public async Task<ActionResult<TagResponseDto>> CreateTag(long projectId, [FromBody] CreateTagRequestDto tagRequestDto)
    {
        try
        {
            var createdTag = await _tagBusiness.CreateTag(projectId, tagRequestDto);
            return CreatedAtAction(nameof(GetTag), new { projectId = projectId, tagId = createdTag.Id }, 
                createdTag);
        }
        catch (Exception exception)
        {
            var message = $"An error occurred while creating tag: {exception}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }
    
    /// <summary>
    /// Creates many tags
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagRequestDto">The tag data transfer object containing tag details.</param>
    /// <returns>The created tag with its details.</returns>
    [HttpPost("BulkCreateTag", Name = "api_create_many_tags")]
    public async Task<ActionResult<List<TagResponseDto>>> BulkCreateTag(
        long projectId, 
        [FromBody] List<CreateTagRequestDto> tagRequestDto)
    {
        try
        {
            var bulkTagResponseDto = await _tagBusiness.BulkCreateTags(projectId, tagRequestDto);
            return Ok(bulkTagResponseDto);
        }
        catch (Exception exception)
        {
            var message = $"An unexpected error occurred while creating these tags: {exception}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    /// Update a tag
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to update.</param>
    /// <param name="tagRequestDto">The tag data transfer object containing updated tag details.</param>
    /// <returns>The updated tag with its details.</returns>
    [HttpPut("UpdateTag/{tagId}", Name = "api_update_a_tag")]
    public async Task<ActionResult<TagResponseDto>> UpdateTag(long projectId, long tagId, [FromBody] UpdateTagRequestDto tagRequestDto)
    {
        try
        {
            var updatedTag = await _tagBusiness.UpdateTag(projectId, tagId, tagRequestDto);
            return Ok(updatedTag);
        }
        catch (Exception exception)
        {
            var message = $"An error occurred while updating tag {tagId}: {exception}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    /// Delete a tag
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to delete.</param>
    /// <returns> A message stating the tag was successfully deleted.</returns>
    [HttpDelete("DeleteTag/{tagId}", Name = "api_delete_a_tag")]
    public async Task<IActionResult> DeleteTag(long projectId, long tagId)
    {
        try
        {
            await _tagBusiness.DeleteTag(projectId, tagId);
            return Ok(new { message = $"Tag deleted successfully" });
        }
        catch (Exception exception)
        {
            var message = $"An error occurred while deleting tag {tagId}: {exception}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }
    
    /// <summary>
    /// Archive a tag 
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to archive.</param>
    /// <returns> A message stating the tag was successfully archived.</returns>
    [HttpDelete("ArchiveTag/{tagId}", Name = "api_archive_a_tag")]
    public async Task<IActionResult> ArchiveTag(long projectId, long tagId)
    {
        try
        {
            await _tagBusiness.ArchiveTag(projectId, tagId);
            return Ok(new { message = $"Tag archived successfully" });
        }
        catch (Exception exception)
        {
            var message = $"An error occurred while archiving tag {tagId}: {exception}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }
    
    /// <summary>
    /// Unarchive a tag 
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to unarchive.</param>
    /// <returns> A message stating the tag was successfully unarchived.</returns>
    [HttpPut("UnarchiveTag/{tagId}", Name = "api_unarchive_a_tag")]
    public async Task<IActionResult> UnarchiveTag(long projectId, long tagId)
    {
        try
        {
            await _tagBusiness.UnarchiveTag(projectId, tagId);
            return Ok(new { message = $"Tag unarchived successfully" });
        }
        catch (Exception exception)
        {
            var message = $"An error occurred while unarchiving tag {tagId}: {exception}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }
}