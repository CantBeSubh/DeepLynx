using deeplynx.helpers.Context;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers;

[ApiController]
[Route("organizations/{organizationId}/labels")]
[Authorize]
[Tags("Organization Management", "Sensitivity Label")]
public class OrganizationSensitivityLabelController : ControllerBase
{
    private readonly ILogger<SensitivityLabelController> _logger;
    private readonly ISensitivityLabelBusiness _sensitivityLabelBusiness;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OrganizationSensitivityLabelController" /> class
    /// </summary>
    /// <param name="sensitivityLabelBusiness">The business logic interface for handling Sensitivity Label operations.</param>
    /// <param name="logger">Error/Info logging interface for database log table.</param>
    public OrganizationSensitivityLabelController(ISensitivityLabelBusiness sensitivityLabelBusiness,
        ILogger<SensitivityLabelController> logger)
    {
        _sensitivityLabelBusiness = sensitivityLabelBusiness;
        _logger = logger;
    }

    /// <summary>
    ///     List Sensitivity Labels
    /// </summary>
    /// <param name="organizationId">ID of the organization across which to search</param>
    /// <param name="projectIds">(Optional)An array of project IDs within the organization to filter by</param>
    /// <param name="hideArchived">Flag indicating whether to hide or show archived labels</param>
    /// <returns></returns>
    [HttpGet(Name = "api_get_all_sensitivity_labels_organization")]
    public async Task<ActionResult<IEnumerable<SensitivityLabelResponseDto>>> GetAllSensitivityLabels(
        long organizationId,
        [FromQuery] long[]? projectIds,
        [FromQuery] bool hideArchived = true)
    {
        try
        {
            var labels = await _sensitivityLabelBusiness
                .GetAllSensitivityLabels(projectIds, organizationId,
                    hideArchived); //setting project ID null for now to circumvent xor logic
            return Ok(labels);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while listing sensitivity labels: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Fetch Sensitivity Label by ID
    /// </summary>
    /// <param name="organizationId">ID of the organization to which the label belongs</param>
    /// <param name="labelId">ID of sensitivity label</param>
    /// <param name="hideArchived">Flag indicating whether to hide or show archived labels</param>
    /// <returns></returns>
    [HttpGet("{labelId}", Name = "api_get_sensitivity_label_organization")]
    public async Task<ActionResult<SensitivityLabelResponseDto>> GetSensitivityLabel(
        long organizationId,
        long labelId, [FromQuery] bool hideArchived = true)
    {
        try
        {
            var label = await _sensitivityLabelBusiness.GetSensitivityLabel(labelId, null, organizationId,
                hideArchived);
            return Ok(label);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while retrieving sensitivity label {labelId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Create a Sensitivity Label
    /// </summary>
    /// <param name="organizationId">ID of the organization to which the label belongs</param>
    /// <param name="dto">Data structure of sensitivity label to create</param>
    /// <returns></returns>
    [HttpPost(Name = "api_create_sensitivity_label_organization")]
    public async Task<ActionResult<SensitivityLabelResponseDto>> CreateSensitivityLabel(
        long organizationId,
        [FromBody] CreateSensitivityLabelRequestDto dto)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;
            var label = await _sensitivityLabelBusiness.CreateSensitivityLabel(currentUserId, dto, null,
                organizationId);
            return Ok(label);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while creating sensitivity label: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Update a Sensitivity Label
    /// </summary>
    /// <param name="organizationId">ID of the organization to which the label belongs</param>
    /// <param name="labelId">ID of the sensitivity label</param>
    /// <param name="dto">Fields to update</param>
    /// <returns></returns>
    [HttpPut("{labelId}", Name = "api_update_sensitivity_label_organization")]
    public async Task<ActionResult<SensitivityLabelResponseDto>> UpdateSensitivityLabel(
        long organizationId,
        long labelId,
        [FromBody] UpdateSensitivityLabelRequestDto dto)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;
            var label = await _sensitivityLabelBusiness.UpdateSensitivityLabel(currentUserId, labelId, null,
                organizationId, dto);
            return Ok(label);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while updating sensitivity label {labelId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Delete a Sensitivity Label
    /// </summary>
    /// <param name="organizationId">ID of the organization to which the label belongs</param>
    /// <param name="labelId">ID of the sensitivity label to hard delete</param>
    /// <returns></returns>
    [HttpDelete("{labelId}", Name = "api_delete_sensitivity_label_organization")]
    public async Task<ActionResult> DeleteSensitivityLabel(
        long organizationId,
        long labelId)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;
            await _sensitivityLabelBusiness.DeleteSensitivityLabel(currentUserId, labelId, null, organizationId);
            return Ok(new { message = $"Deleted sensitivity label {labelId}" });
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while deleting sensitivity label {labelId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Archive or Unarchive a Sensitivity Label
    /// </summary>
    /// <param name="organizationId">ID of the organization to which the label belongs</param>
    /// <param name="labelId">The ID of the sensitivity label to archive or unarchive.</param>
    /// <param name="archive">True to archive the label, false to unarchive it.</param>
    /// <returns>A message stating the label was successfully archived or unarchived.</returns>
    [HttpPatch("{labelId}", Name = "api_archive_sensitivity_label_organization")]
    public async Task<IActionResult> ArchiveSensitivityLabel(
        long organizationId,
        long labelId,
        [FromQuery] bool archive)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;
            if (archive)
            {
                await _sensitivityLabelBusiness.ArchiveSensitivityLabel(currentUserId, labelId, null,
                    organizationId);
                return Ok(new { message = $"Archived sensitivity label {labelId}" });
            }

            await _sensitivityLabelBusiness.UnarchiveSensitivityLabel(currentUserId, labelId, null,
                organizationId);
            return Ok(new { message = $"Unarchived sensitivity label {labelId}" });
        }
        catch (Exception exc)
        {
            var action = archive ? "archiving" : "unarchiving";
            var message = $"An error occurred while {action} sensitivity label {labelId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }
}