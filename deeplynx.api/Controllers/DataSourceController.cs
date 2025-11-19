using deeplynx.helpers.Context;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("projects/{projectId}/datasources")]
    [Authorize]
    public class DataSourceController : ControllerBase
    {
        private readonly IDataSourceBusiness _dataSourceBusiness;
        private readonly ILogger<DataSourceController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceController"/> class
        /// </summary>
        /// <param name="dataSourceBusiness">The business logic interface for handling data source operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public DataSourceController(IDataSourceBusiness dataSourceBusiness, ILogger<DataSourceController> logger)
        {
            _dataSourceBusiness = dataSourceBusiness;
            _logger = logger;
        }

        /// <summary>
        /// Get all data sources
        /// </summary>
        /// <param name="organizationId">The Organization ID to which the data source belongs</param>
        /// <param name="projectId">The ID of the project whose data sources are to be retrieved</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived data sources from the result (Default true)</param>
        /// <returns>A list of data sources for the given project.</returns>
        [HttpGet("GetAllDataSources", Name = "api_get_all_data_sources")]
        public async Task<ActionResult<IEnumerable<DataSourceResponseDto>>> GetAllDataSources(long projectId, long organizationId,
           [FromQuery] bool hideArchived = true)
        {
            try
            {
                var dataSources = await _dataSourceBusiness.GetAllDataSources(projectId, organizationId, hideArchived);
                return Ok(dataSources);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing all data sources: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Get a data source
        /// </summary>
        /// <param name="organizationId">The Organization ID to which the data source belongs</param>
        /// <param name="dataSourceId">The ID whereby to fetch the data source</param>
        /// <param name="projectId">The ID of the project to which the data source belongs</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived data sources from the result (Default true)</param>
        /// <returns>The data source associated with the given ID</returns>
        [HttpGet("GetDataSource/{dataSourceId}", Name = "api_get_a_data_source")]
        public async Task<ActionResult<DataSourceResponseDto>> GetDataSource(
            long organizationId,
            long projectId,
            long dataSourceId,
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var dataSource = await _dataSourceBusiness.GetDataSource(organizationId, projectId, dataSourceId, hideArchived);
                return Ok(dataSource);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving data source {dataSourceId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Get default data source
        /// </summary>
        /// <param name="organizationId">The Organization ID to which the data source belongs</param>
        /// <param name="projectId">The ID of the project to which the data source belongs</param>
        /// <returns>The data source associated with the given ID</returns>
        [HttpGet("GetDefaultDataSource", Name = "api_get_default_data_source")]
        public async Task<ActionResult<DataSourceResponseDto>> GetDefaultDataSource(
            long organizationId,
            long projectId)
        {
            try
            {
                var dataSource = await _dataSourceBusiness.GetDefaultDataSource(organizationId, projectId);
                return Ok(dataSource);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving default data source for project {projectId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Create a data source 
        /// </summary>
        /// <param name="organizationId">The Organization ID to which the data source belongs</param>
        /// <param name="projectId">The ID of the project to which the data source belongs</param>
        /// <param name="dto">The data transfer object containing data source details</param>
        /// <returns>The created data source</returns>
        [HttpPost("CreateDataSource", Name = "api_create_a_data_source")]
        public async Task<ActionResult<DataSourceResponseDto>> CreateDataSource(
            long organizationId,
            long projectId,
            [FromBody] CreateDataSourceRequestDto dto)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var dataSource = await _dataSourceBusiness.CreateDataSource(currentUserId, organizationId, projectId, dto);
                return Ok(dataSource);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating data source: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Update a data source
        /// </summary>
        /// <param name="organizationId">The Organization ID to which the data source belongs</param>
        /// <param name="dataSourceId">The ID of the data source to update</param>
        /// <param name="projectId">The ID of the project to which the data source belongs</param>
        /// <param name="dto">The data transfer object containing updated data source details</param>
        /// <returns>The newly updated data source</returns>
        [HttpPut("UpdateDataSource/{dataSourceId}", Name = "api_update_a_data_source")]
        public async Task<ActionResult<DataSourceResponseDto>> UpdateDataSource(
            long organizationId,
            long projectId,
            long dataSourceId,
            [FromBody] UpdateDataSourceRequestDto dto)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var dataSource = await _dataSourceBusiness.UpdateDataSource(currentUserId, organizationId, projectId, dataSourceId, dto);
                return Ok(dataSource);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating data source {dataSourceId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Deletes a data source
        /// </summary>
        /// <param name="organizationId">The Organization ID to which the data source belongs</param>
        /// <param name="dataSourceId">The ID of the data source to delete.</param>
        /// <param name="projectId">The ID of the project to which the data source belongs.</param>
        /// <returns>A message stating the data source was successfully deleted.</returns>
        [HttpDelete("DeleteDataSource/{dataSourceId}", Name = "api_delete_a_data_source")]
        public async Task<IActionResult> DeleteDataSource(
            long organizationId,
            long dataSourceId,
            long projectId)
        {
            try
            {
                await _dataSourceBusiness.DeleteDataSource(organizationId, projectId, dataSourceId);
                return Ok(new { message = $"Deleted data source {dataSourceId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting data source {dataSourceId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Archive a data source 
        /// </summary>
        /// <param name="organizationId">The Organization ID to which the data source belongs</param>
        /// <param name="dataSourceId">The ID of the data source to archive.</param>
        /// <param name="projectId">The ID of the project to which the data source belongs.</param>
        /// <returns>A message stating the data source was successfully archived.</returns>
        [HttpDelete("ArchiveDataSource/{dataSourceId}", Name = "api_archive_a_data_source")]
        public async Task<IActionResult> ArchiveDataSource(
            long organizationId, 
            long dataSourceId, 
            long projectId)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                await _dataSourceBusiness.ArchiveDataSource(currentUserId, organizationId, projectId, dataSourceId);
                return Ok(new { message = $"Archived data source {dataSourceId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving data source {dataSourceId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Unarchive a data source 
        /// </summary>
        /// <param name="organizationId">The Organization ID to which the data source belongs</param>
        /// <param name="dataSourceId">The ID of the data source to unarchive.</param>
        /// <param name="projectId">The ID of the project to which the data source belongs.</param>
        /// <returns>A message stating the data source was successfully unarchived.</returns>
        [HttpPut("UnarchiveDataSource/{dataSourceId}", Name = "api_unarchive_a_data_source")]
        public async Task<IActionResult> UnarchiveDataSource(
            long organizationId, 
            long dataSourceId, 
            long projectId)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                await _dataSourceBusiness.UnarchiveDataSource(currentUserId, organizationId, projectId, dataSourceId);
                return Ok(new { message = $"Unarchived data source {dataSourceId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while unarchiving data source {dataSourceId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Set default data source
        /// </summary>
        /// <param name="organizationId">The Organization ID to which the data source belongs</param>
        /// <param name="dataSourceId">The ID of the data source to update</param>
        /// <param name="projectId">The ID of the project to which the data source belongs</param>
        /// <returns>The newly updated data source</returns>
        [HttpPut("SetDefaultDataSource/{dataSourceId}", Name = "api_set_default_data_source")]
        public async Task<ActionResult<DataSourceResponseDto>> SetDefaultDataSource(
            long organizationId,
            long projectId,
            long dataSourceId)
        {
            try
            {
                var currentUserId = UserContextStorage.UserId;
                var dataSource = await _dataSourceBusiness.SetDefaultDataSource(currentUserId, organizationId, projectId, dataSourceId);
                return Ok(dataSource);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while setting default data source {dataSourceId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}