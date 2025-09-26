using deeplynx.helpers;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers;

[Route("api/projects/{projectId}/datasources/{dataSourceId}/metadata")]
[ApiController]
[NexusAuthorize]
public class MetadataController : ControllerBase
{
    private readonly IMetadataBusiness _metadataBusiness;
    private readonly ILogger<MetadataController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataController"/> class.
    /// </summary>
    /// <param name="metadataBusiness">The business logic interface for handling metadata operations.</param>
    /// <param name="logger">Error/Info logging interface for database log table.</param>
    public MetadataController(IMetadataBusiness metadataBusiness, ILogger<MetadataController> logger)
    {
        _metadataBusiness = metadataBusiness;
        _logger = logger;
    }

    /// <summary>
    /// Parses metadata
    /// </summary>
    /// <param name="projectId">The ID of the project to which the metadata belongs.</param>
    /// <param name="dataSourceId">The ID of the datasource from which the metadata was collected.</param>
    /// <param name="metadataRequestDto">The metadata data transfer object containing metadata details.</param>
    [HttpPost("CreateMetadata", Name = "api_create_metadata")]
    public async Task<ActionResult<MetadataResponseDto>> CreateMetadata(
        long projectId, 
        long dataSourceId,
        [FromBody] CreateMetadataRequestDto metadataRequestDto)
    {
        try
        {
            var createdMetadata = await _metadataBusiness.CreateMetadata(projectId, dataSourceId, metadataRequestDto);
            return Ok(createdMetadata);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            var message = $"An error occurred while parsing metadata: {exception}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }
}