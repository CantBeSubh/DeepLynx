using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers;

[Route("api/projects/{projectId}/metadata")]
[ApiController]
public class MetadataController : ControllerBase
{
    private readonly IMetadataBusiness _metadataBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataController"/> class.
    /// </summary>
    /// <param name="metadataBusiness">The business logic interface for handling metadata operations.</param>
    public MetadataController(IMetadataBusiness metadataBusiness)
    {
        _metadataBusiness = metadataBusiness;
    }

    /// <summary>
    /// Parses metadata
    /// </summary>
    /// <param name="projectId">The ID of the project to which the metadata belongs.</param>
    /// <param name="dataSourceId">The ID of the data source to which the metadata belongs.</param>
    /// <param name="metadataRequestDto">The metadata data transfer object containing metadata details.</param>
    [HttpPost("CreateMetadata", Name = "api_create_metadata")]
    public async Task<ActionResult<MetadataResponseDto>> CreateMetadata(
        long projectId, 
        long dataSourceId,
        [FromBody] MetadataRequestDto metadataRequestDto)
    {
        try
        {
            var createdMetadata = await _metadataBusiness.CreateMetadata(projectId, dataSourceId, metadataRequestDto);
            return StatusCode(StatusCodes.Status201Created, "Your metadata has been received.");
        }
        catch (Exception exception)
        {
            var message = $"An error occurred while parsing metadata: {exception}";
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }
}