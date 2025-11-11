using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;
using deeplynx.helpers;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("labels")]
    public class SensitivityLabelController : ControllerBase
    {
        private readonly ISensitivityLabelBusiness _sensitivityLabelBusiness;
        private readonly ILogger<SensitivityLabelController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SensitivityLabelController"/> class
        /// </summary>
        /// <param name="sensitivityLabelBusiness">The business logic interface for handling Sensitivity Label operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public SensitivityLabelController(ISensitivityLabelBusiness sensitivityLabelBusiness, ILogger<SensitivityLabelController> logger)
        {
            _sensitivityLabelBusiness = sensitivityLabelBusiness;
            _logger = logger;
        }

        /// <summary>
        /// List Sensitivity Labels
        /// </summary>
        /// <param name="projectId">(optional) ID of the project across which to search</param>
        /// <param name="organizationId">(optional) ID of the organization across which to search</param>
        /// <param name="hideArchived">Flag indicating whether to hide or show archived labels</param>
        /// <returns></returns>
        [HttpGet("GetAllSensitivityLabels", Name = "api_get_all_sensitivity_labels")]
        [AuthInProject("read", "sensitivity_label")]
        public async Task<ActionResult<IEnumerable<SensitivityLabelResponseDto>>> GetAllSensitivityLabels(
            [FromQuery] long? projectId = null,
            [FromQuery] long? organizationId = null,
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var labels = await _sensitivityLabelBusiness
                    .GetAllSensitivityLabels(projectId, organizationId, hideArchived);
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
        /// Fetch Sensitivity Label by ID
        /// </summary>
        /// <param name="labelId">ID of sensitivity label</param>
        /// <param name="hideArchived">Flag indicating whether to hide or show archived labels</param>
        /// <returns></returns>
        [HttpGet("GetSensitivityLabel/{labelId}", Name = "api_get_sensitivity_label")]
        [AuthInProject("read", "sensitivity_label")]
        public async Task<ActionResult<SensitivityLabelResponseDto>> GetSensitivityLabel(
            long labelId, [FromQuery] bool hideArchived = true)
        {
            try
            {
                var label = await _sensitivityLabelBusiness.GetSensitivityLabel(labelId, hideArchived);
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
        /// Create a Sensitivity Label
        /// </summary>
        /// <param name="dto">Data structure of sensitivity label to create</param>
        /// <param name="projectId">(use this or org ID) ID of the project to which the label belongs</param>
        /// <param name="organizationId">(use this or project ID) ID of the organization to which the label belongs</param>
        /// <returns></returns>
        [HttpPost("CreateSensitivityLabel", Name = "api_create_sensitivity_label")]
        [AuthInProject("write", "sensitivity_label")]
        public async Task<ActionResult<SensitivityLabelResponseDto>> CreateSensitivityLabel(
            [FromBody] CreateSensitivityLabelRequestDto dto,
            [FromQuery] long? projectId = null,
            [FromQuery] long? organizationId = null)
        {
            try
            {
                var label = await _sensitivityLabelBusiness.CreateSensitivityLabel(dto, projectId, organizationId);
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
        /// Update a Sensitivity Label
        /// </summary>
        /// <param name="labelId">ID of the sensitivity label</param>
        /// <param name="dto">Fields to update</param>
        /// <returns></returns>
        [HttpPut("UpdateSensitivityLabel/{labelId}", Name = "api_update_sensitivity_label")]
        [AuthInProject("write", "sensitivity_label")]
        public async Task<ActionResult<SensitivityLabelResponseDto>> UpdateSensitivityLabel(
            long labelId,
            [FromBody] UpdateSensitivityLabelRequestDto dto)
        {
            try
            {
                var label = await _sensitivityLabelBusiness.UpdateSensitivityLabel(labelId, dto);
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
        /// Delete a Sensitivity Label
        /// </summary>
        /// <param name="labelId">ID of the sensitivity label to hard delete</param>
        /// <returns></returns>
        [HttpDelete("DeleteSensitivityLabel/{labelId}", Name = "api_delete_sensitivity_label")]
        [AuthInProject("write", "sensitivity_label")]
        public async Task<ActionResult> DeleteSensitivityLabel(long labelId)
        {
            try
            {
                await _sensitivityLabelBusiness.DeleteSensitivityLabel(labelId);
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
        /// Archive a Sensitivity Label
        /// </summary>
        /// <param name="labelId">ID of the sensitivity label</param>
        /// <returns></returns>
        [HttpDelete("ArchiveSensitivityLabel/{labelId}", Name = "api_archive_sensitivity_label")]
        [AuthInProject("write", "sensitivity_label")]
        public async Task<ActionResult> ArchiveSensitivityLabel(long labelId)
        {
            try
            {
                await _sensitivityLabelBusiness.ArchiveSensitivityLabel(labelId);
                return Ok(new { message = $"Archived sensitivity label {labelId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving sensitivity label {labelId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Unarchive a Sensitivity Label
        /// </summary>
        /// <param name="labelId">ID of the sensitivity label</param>
        /// <returns></returns>
        [HttpPut("UnarchiveSensitivityLabel/{labelId}", Name = "api_unarchive_sensitivity_label")]
        [AuthInProject("write", "sensitivity_label")]
        public async Task<ActionResult> UnarchiveSensitivityLabel(long labelId)
        {
            try
            {
                await _sensitivityLabelBusiness.UnarchiveSensitivityLabel(labelId);
                return Ok(new { message = $"Unarchived sensitivity label {labelId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while unarchiving sensitivity label {labelId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}