using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    
    /// <summary>
    /// Controller for managing classes.
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints to create, update, delete, and retrieve class information.
    /// </remarks>

    [ApiController]
    [Route("api/records")]
    public class QueryController : ControllerBase
    {
        private readonly IQueryBusiness _queryBusiness;

        public QueryController(IQueryBusiness queryBusiness)
        {
            _queryBusiness = queryBusiness;
        }
        /// <summary>
        /// Full text search for records
        /// </summary>
        /// <param name="userQuery">String phrase entered by user</param>
        /// <returns>List of historical record response DTOs</returns>
        [HttpGet("Filter", Name = "api_filter_records")]
        public async Task<ActionResult<IEnumerable<HistoricalRecordResponseDto>>> SearchRecords(
            [FromQuery] string userQuery)
        {
            try
            {
                var records = await _queryBusiness.Search(userQuery);
                return Ok(records);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while searching for records.: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        
        }
        
        
        /// <summary>
        /// Build a query for records
        /// </summary>
        /// <param name="filterArray">Array of QueryComponent dtos</param>
        /// <returns>List of historical record response DTOs</returns>
        [HttpPost("BuildAQuery")]
        public async Task<ActionResult<IEnumerable<HistoricalRecordResponseDto>>> BuildAQuery(
            [FromBody] CustomQueryRequestDto[] filterArray, string initialQuery)
        {
            try
            {
                var records = _queryBusiness.BuildAQuery(filterArray);
                return Ok(records);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while searching for records.: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

        }
        
    }
}