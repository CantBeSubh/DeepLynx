using deeplynx.models;
using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using NLog;
using System.Threading.Tasks;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/graph")]
    public class KuzuDatabaseManagerController : ControllerBase
    {
        private readonly IKuzuDatabaseManager _kuzuDatabaseManager;

        public KuzuDatabaseManagerController(IKuzuDatabaseManager kuzuDatabaseManager)
        {
            _kuzuDatabaseManager = kuzuDatabaseManager;
        }

        /// <summary>
        /// Query Kuzu database
        /// </summary>
        /// <param name="request">The SQL query string to execute</param>
        /// <returns>A result set from the Kuzu database</returns>
        [HttpPost("query")]
        public async Task<IActionResult> QueryKuzuDatabase([FromBody] KuzuDatabaseManagerQueryRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {

                await _kuzuDatabaseManager.ConnectAsync();
                string pgParams = "dbname=deeplynx user=postgres host=localhost password=password port=5432";
                int project_id = request.ProjectId;

                await _kuzuDatabaseManager.ExportDataAsync(pgParams, project_id);

                var result = await Task.Run(() => _kuzuDatabaseManager.ExecuteQueryAsync(request));
                return Ok(result);
            }
            catch (Exception e)
            {
                var message = $"An error occurred while querying the Kuzu database: {e.Message}";
                LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}