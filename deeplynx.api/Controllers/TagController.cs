using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers;

[Route("api/projects/{projectId}/tags")]
[ApiController]
public class TagController : ControllerBase
{
    private readonly ITagBusiness _tagBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagController"/> class.
    /// </summary>
    /// <param name="tagBusiness">The business logic interface for handling tag operations.</param>
    public TagController(ITagBusiness tagBusiness)
    {
        _tagBusiness = tagBusiness;
    }

    /// <summary>
    /// Creates a new tag for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagRequestDto">The tag data transfer object containing tag details.</param>
    /// <returns>The created tag with its details.</returns>
    [HttpPost("CreateTag")]
    public async Task<ActionResult<TagResponseDto>> CreateTag(long projectId, [FromBody] TagRequestDto tagRequestDto)
    {
        try
        {
            var createdTag = await _tagBusiness.CreateTag(projectId, tagRequestDto);
            return CreatedAtAction(nameof(GetTagById), new { projectId = projectId, tagId = createdTag.Id }, 
                createdTag);
        }
        catch (Exception exception)
        {
            var message = $"An error occurred while creating tag: {exception}";
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    /// Updates an existing tag for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to update.</param>
    /// <param name="tagRequestDto">The tag data transfer object containing updated tag details.</param>
    /// <returns>The updated tag with its details.</returns>
    [HttpPut("UpdateTag/{tagId}")]
    public async Task<ActionResult<TagResponseDto>> UpdateTag(long projectId, long tagId, [FromBody] TagRequestDto tagRequestDto)
    {
        try
        {
            var updatedTag = await _tagBusiness.UpdateTag(projectId, tagId, tagRequestDto);
            return Ok(updatedTag);
        }
        catch (Exception exception)
        {
            var message = $"An error occurred while updating tag {tagId}: {exception}";
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    /// Retrieves all tags for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project whose tags are to be retrieved.</param>
    /// <returns>A list of tags belonging to the project.</returns>
    [HttpGet("GetAllTags")]
    public async Task<ActionResult<IEnumerable<TagResponseDto>>> GetAllTags(long projectId)
    {
        try
        {
            var tags = await _tagBusiness.GetAllTags(projectId);
            return Ok(tags);
        }
        catch (Exception exception)
        {
            var message = $"An error occurred while listing all tags: {exception}";
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    /// Retrieves a specific tag by its ID for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to retrieve.</param>
    /// <returns>The tag with its details.</returns>
    [HttpGet("GetTagById/{tagId}")]
    public async Task<ActionResult<TagResponseDto>> GetTagById(long projectId, long tagId)
    {
        try
        {
            var tag = await _tagBusiness.GetTagById(projectId, tagId);
            return Ok(tag);
        }
        catch (Exception exception)
        {
            var message = $"An error occurred while retrieving tag {tagId}: {exception}";
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    /// Deletes a specific tag by its ID for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to delete.</param>
    /// <returns> A message stating the tag was successfully deleted.</returns>
    [HttpDelete("DeleteTag/{tagId}")]
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
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }
    
    /// <summary>
    /// Archives a specific tag by its ID for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to archive.</param>
    /// <returns> A message stating the tag was successfully archived.</returns>
    [HttpDelete("ArchiveTag/{tagId}")]
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
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }
}