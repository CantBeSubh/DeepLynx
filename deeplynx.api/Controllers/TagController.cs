using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers;

[Route("projects/{projectId}/tags")]
[ApiController]
public class TagsController : ControllerBase
{
    private readonly ITagBusiness _tagBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagsController"/> class.
    /// </summary>
    /// <param name="tagBusiness">The business logic interface for handling tag operations.</param>
    public TagsController(ITagBusiness tagBusiness)
    {
        _tagBusiness = tagBusiness;
    }

    /// <summary>
    /// Creates a new tag for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="TagRequestDto">The tag data transfer object containing tag details.</param>
    /// <returns>The created tag with its details.</returns>
    [HttpPost("CreateTag")]
    public async Task<ActionResult<TagRequestDto>> CreateTag(long projectId, [FromBody] TagRequestDto TagRequestDto)
    {
        var createdTag = await _tagBusiness.CreateTagAsync(projectId, TagRequestDto);
        return CreatedAtAction(nameof(GetTagById), new { projectId = projectId, tagId = createdTag.Id }, createdTag);
    }

    /// <summary>
    /// Updates an existing tag for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to update.</param>
    /// <param name="TagRequestDto">The tag data transfer object containing updated tag details.</param>
    /// <returns>The updated tag with its details.</returns>
    [HttpPut("UpdateTag/{tagId}")]
    public async Task<ActionResult<TagRequestDto>> UpdateTag(long projectId, long tagId, [FromBody] TagRequestDto TagRequestDto)
    {
        var updatedTag = await _tagBusiness.UpdateTagAsync(projectId, tagId, TagRequestDto);
        return Ok(updatedTag);
    }

    /// <summary>
    /// Retrieves all tags for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project whose tags are to be retrieved.</param>
    /// <returns>A list of tags belonging to the project.</returns>
    [HttpGet("GetAllTags")]
    public async Task<ActionResult<IEnumerable<TagRequestDto>>> GetAllTags(long projectId)
    {
        var tags = await _tagBusiness.GetAllTagsAsync(projectId);
        return Ok(tags);
    }

    /// <summary>
    /// Retrieves a specific tag by its ID for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to retrieve.</param>
    /// <returns>The tag with its details.</returns>
    [HttpGet("GetTagById/{tagId}")]
    public async Task<ActionResult<TagRequestDto>> GetTagById(long projectId, long tagId)
    {
        var tag = await _tagBusiness.GetTagByIdAsync(projectId, tagId);
        return Ok(tag);
    }

    /// <summary>
    /// Deletes a specific tag by its ID for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to delete.</param>
    /// <param name="force">Indicates whether to force delete the tag if it is in use.</param>
    /// <returns>No content if the tag is successfully deleted.</returns>
    [HttpDelete("DeleteTag/{tagId}")]
    public async Task<IActionResult> DeleteTag(long projectId, long tagId, [FromQuery] bool force = false)
    {
        await _tagBusiness.DeleteTagAsync(projectId, tagId, force);
        return NoContent();
    }
}