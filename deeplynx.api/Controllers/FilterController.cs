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
    public class FilterController : ControllerBase
    {
        private readonly IFilterBusiness _filterBusiness;

        public FilterController(IFilterBusiness filterBusiness)
        {
            _filterBusiness = filterBusiness;
        }
        /// <summary>
        /// Filter records 
        /// </summary>
        /// <param name="dto">The filter request dto</param>
        /// <returns>List of class response DTOs</returns>
        [HttpPost("Filter")]
        public async Task<ActionResult<IEnumerable<RecordResponseDto>>> FilterRecords(
            [FromBody] FilterRequestDto dto)
        {
            try
            {
                var records = await _filterBusiness.FilterRecords(dto);
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