using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers;

[Route("projects/{projectId}/tags")]
[ApiController]
public class TagsController : ControllerBase
{
    private readonly ITagBusiness _tagBusiness;

    public TagsController(ITagBusiness tagBusiness)
    {
        _tagBusiness = tagBusiness;
    }

    [HttpPost("CreateTag")]
    public async Task<ActionResult<TagRequestDto>> CreateTag(long projectId, [FromBody] TagRequestDto TagRequestDto)
    {
        var createdTag = await _tagBusiness.CreateTagAsync(projectId, TagRequestDto);
        return CreatedAtAction(nameof(GetTagById), new { projectId = projectId, tagId = createdTag.Id }, createdTag);
    }

    [HttpPut("UpdateTag/{tagId}")]
    public async Task<ActionResult<TagRequestDto>> UpdateTag(long projectId, long tagId, [FromBody] TagRequestDto TagRequestDto)
    {
        var updatedTag = await _tagBusiness.UpdateTagAsync(projectId, tagId, TagRequestDto);
        return Ok(updatedTag);
    }

    [HttpGet("GetAllTags")]
    public async Task<ActionResult<IEnumerable<TagRequestDto>>> GetAllTags(long projectId)
    {
        var tags = await _tagBusiness.GetAllTagsAsync(projectId);
        return Ok(tags);
    }

    [HttpGet("GetTagById/{tagId}")]
    public async Task<ActionResult<TagRequestDto>> GetTagById(long projectId, long tagId)
    {
        var tag = await _tagBusiness.GetTagByIdAsync(projectId, tagId);
        return Ok(tag);
    }

    [HttpDelete("DeleteTag/{tagId}")]
    public async Task<IActionResult> DeleteTag(long projectId, long tagId, [FromQuery] bool force = false)
    {
        await _tagBusiness.DeleteTagAsync(projectId, tagId, force);
        return NoContent();
    }
}