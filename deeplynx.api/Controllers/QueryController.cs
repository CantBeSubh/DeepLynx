using deeplynx.helpers.Context;
using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers
{

    /// <summary>
    /// Controller for managing classes.
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints to create, update, delete, and retrieve class information.
    /// </remarks>

    [ApiController]
    [Route("records")]
    [Authorize]
    public class QueryController : ControllerBase
    {
        private readonly IQueryBusiness _queryBusiness;
        private readonly ILogger<QueryController> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryBusiness">The business logic interface for handling querying operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public QueryController(IQueryBusiness queryBusiness, ILogger<QueryController> logger)
        {
            _queryBusiness = queryBusiness;
            _logger = logger;
        }

        /// <summary>
        /// Full text search for records
        /// </summary>
        /// <param name="userQuery">String phrase entered by user</param>
        /// /// <param name="projectIds">Project ids that a user has access to</param>
        /// <returns>List of historical record response DTOs</returns>
        [HttpGet("Filter", Name = "api_filter_records")]
        public async Task<ActionResult<IEnumerable<HistoricalRecordResponseDto>>> SearchRecords(
            [FromQuery] string userQuery, [FromQuery] long[] projectIds)
        {
            try
            {
                var records = await _queryBusiness.Search(userQuery, projectIds);
                return Ok(records);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while searching for records.: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

        }


        /// <summary>
        /// Build a query for records
        /// </summary>
        /// <param name="filterArray">Array of QueryComponent dtos</param>
        /// <param name="textSearch">Full text search phrase</param>
        /// /// <param name="projectIds">Project ids that a user has access to</param>
        /// <returns>List of historical record response DTOs</returns>
        [HttpPost("QueryBuilder", Name = "api_query_builder_records")]
        public async Task<ActionResult<IEnumerable<HistoricalRecordResponseDto>>> QueryBuilder(
            [FromQuery] string? textSearch, [FromQuery] long[] projectIds,
            [FromBody] CustomQueryDtos.CustomQueryRequestDto[] filterArray)
        {
            try
            {
                var records = await _queryBusiness.QueryBuilder(filterArray, projectIds, textSearch);
                return Ok(records);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while searching for records.: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

        }

        /// <summary>
        /// Get all classes
        /// </summary>
        /// <param name="projectIds">The IDs of the projects to which the class belongs</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived classes from the result (Default true)</param>
        /// <returns>List of class response DTOs</returns>
        [HttpGet("GetAllClasses", Name = "api_query_class_records")]
        public async Task<ActionResult<IEnumerable<ClassResponseDto>>> GetAllClasses(
            [FromQuery] long[] projectIds, bool hideArchived = true)
        {
            try
            {
                var classes = await _queryBusiness.GetAllClasses(projectIds, hideArchived);
                return Ok(classes);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while fetching all classes.: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

        }

        /// <summary>
        /// Get all data sources
        /// </summary>
        /// <param name="projectIds">The IDs of the projects whose data sources are to be retrieved</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived data sources from the result (Default true)</param>
        /// <returns>A list of data sources for the given project.</returns>
        [HttpGet("GetAllDataSources", Name = "api_query_data_source_records")]
        public async Task<ActionResult<IEnumerable<DataSourceResponseDto>>> GetAllDataSources(
            [FromQuery] long[] projectIds, bool hideArchived = true)
        {
            try
            {
                var dataSources = await _queryBusiness.GetAllDataSources(projectIds, hideArchived);
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
        /// Get all tags
        /// </summary>
        /// <param name="projectIds">The IDs of the projects whose tags are to be retrieved.</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived tags from the result (Default true)</param>
        /// <returns>A list of tags belonging to the project.</returns>
        [HttpGet("GetAllTags", Name = "api_query_tag_records")]
        public async Task<ActionResult<IEnumerable<TagResponseDto>>> GetAllTags(
            [FromQuery] long[] projectIds, bool hideArchived = true)
        {
            try
            {
                var tags = await _queryBusiness.GetAllTags(projectIds, hideArchived);
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
        /// Saved searches
        /// </summary>
        /// <param name="filterArray">Array of QueryComponent dtos</param>
        /// <param name="textSearch">Full text search phrase</param>
        /// <param name="alias">Name for saved search</param>
        /// <param name="favorite">Boolean for if favorite search or not</param>
        /// <returns>True if successfully saved</returns>
        [HttpPost("SavedSearch", Name = "api_save_search")]
        public async Task<ActionResult<bool>> SaveSearch(
            [FromQuery] string? textSearch, [FromQuery] string? alias, [FromQuery] bool favorite,
            [FromBody] CustomQueryDtos.CustomQueryRequestDto[] filterArray)
        {
            try
            {
                // get user ID from the middleware context
                var currentUserId = UserContextStorage.UserId;
                var result = await _queryBusiness.SaveSearch(currentUserId, alias, textSearch, filterArray, favorite);
                return Ok(result);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while searching for records.: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Get saved searches
        /// </summary>
        /// <returns>A list of saved searches belonging to the user.</returns>
        [HttpGet("GetSavedSearches", Name = "api_query_get_saved_searches")]
        public async Task<ActionResult<IEnumerable<TagResponseDto>>> GetSavedSearches()
        {
            try
            {
                // get user ID from the middleware context
                var currentUserId = UserContextStorage.UserId;
                var savedSearches = await _queryBusiness.GetSavedSearches(currentUserId);
                return Ok(savedSearches);
            }
            catch (Exception exception)
            {
                var message = $"An error occurred while listing all saved searches: {exception}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

    }
}
