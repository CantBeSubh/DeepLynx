using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;
using deeplynx.business;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/datasources")]
    public class DataSourceController : ControllerBase
    {
        private readonly IDataSourceBusiness _dataSourceBusiness;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceController"/> class
        /// </summary>
        /// <param name="dataSourceBusiness">The business logic interface for handling data source operations.</param>
        public DataSourceController(IDataSourceBusiness dataSourceBusiness)
        {
            _dataSourceBusiness = dataSourceBusiness;
        }

        /// <summary>
        /// Retrieves all data sources for a specific project.
        /// </summary>
        /// <param name="projectId">The ID of the project whose data sources are to be retrieved</param>
        /// <returns>A list of data sources for the given project.</returns>
        [HttpGet("GetAllDataSources")]
        public async Task<IActionResult> GetAllDataSources(long projectId)
        {
            try
            {
                var dataSources = await _dataSourceBusiness.GetAllDataSources(projectId);
                return Ok(dataSources);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing all data sources: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Retrieves a specific data source by ID
        /// </summary>
        /// <param name="dataSourceId">The ID whereby to fetch the data source</param>
        /// <param name="projectId">The ID of the project to which the data source belongs</param>
        /// <returns>The data source associated with the given ID</returns>
        [HttpGet("GetDataSource/{dataSourceId}")]
        public async Task<IActionResult> GetDataSource(long projectId, long dataSourceId)
        {
            try
            {
                var dataSource = await _dataSourceBusiness.GetDataSource(projectId, dataSourceId);
                return Ok(dataSource);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving data source {dataSourceId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Asynchronously creates a new data source for a specified project.
        /// </summary>
        /// <param name="projectId">The ID of the project to which the data source belongs</param>
        /// <param name="dto">The data transfer object containing data source details</param>
        /// <returns>The created data source</returns>
        [HttpPost("CreateDataSource")]
        public async Task<IActionResult> CreateDataSource(long projectId, [FromBody] DataSourceRequestDto dto)
        {
            try
            {
                var dataSource = await _dataSourceBusiness.CreateDataSource(projectId, dto);
                return Ok(dataSource);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating data source: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Asynchronously updates an existing data source.
        /// </summary>
        /// <param name="dataSourceId">The ID of the data source to update</param>
        /// <param name="projectId">The ID of the project to which the data source belongs</param>
        /// <param name="dto">The data transfer object containing updated data source details</param>
        /// <returns>The newly updated data source</returns>
        [HttpPut("UpdateDataSource/{dataSourceId}")]
        public async Task<ActionResult<DataSourceResponseDto>> UpdateDataSource(
            long projectId,
            long dataSourceId,
            [FromBody] DataSourceRequestDto dto)
        {
            try
            {
                var dataSource = await _dataSourceBusiness.UpdateDataSource(projectId, dataSourceId, dto);
                return Ok(dataSource);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating data source {dataSourceId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Deletes a specific data source by its ID.
        /// </summary>
        /// <param name="dataSourceId">The ID of the data source to delete.</param>
        /// <param name="projectId">The ID of the project to which the data source belongs.</param>
        /// <returns>A message stating the data source was successfully deleted.</returns>
        [HttpDelete("DeleteDataSource/{dataSourceId}")]
        public async Task<IActionResult> DeleteDataSource(
            long dataSourceId,
            long projectId)
        {
            try
            {
                await _dataSourceBusiness.DeleteDataSource(projectId, dataSourceId);
                return Ok(new { message = $"Deleted data source {dataSourceId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting data source {dataSourceId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Deletes a specific data source by its ID.
        /// </summary>
        /// <param name="dataSourceId">The ID of the data source to delete.</param>
        /// <param name="projectId">The ID of the project to which the data source belongs.</param>
        /// <returns>A message stating the data source was successfully deleted.</returns>
        [HttpDelete("ArchiveDataSource/{dataSourceId}")]
        public async Task<IActionResult> ArchiveDataSource(
            long dataSourceId, 
            long projectId)
        {
            try
            {
                await _dataSourceBusiness.ArchiveDataSource(projectId, dataSourceId);
                return Ok(new { message = $"Archived data source {dataSourceId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving data source {dataSourceId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}