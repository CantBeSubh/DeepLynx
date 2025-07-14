using deeplynx.models;
using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using NLog;
using System.Threading.Tasks;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/graph")]
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
        /// <param name="projectId">The ID of the project to export from PostgreSQL into the KuzuDB</param>
        /// <param name="request">The SQL query string to execute</param>
        /// <returns>A result set from the Kuzu database</returns>
        [HttpPost("Query")]
        public async Task<IActionResult> QueryKuzuDatabase(int projectId, [FromBody] KuzuDBMQueryRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _kuzuDatabaseManager.ConnectAsync();

                await _kuzuDatabaseManager.ExportDataAsync(projectId);

                var result = await _kuzuDatabaseManager.ExecuteQueryAsync(request);
                return Ok(result);
            }
            catch (Exception e)
            {
                var message = $"An error occurred while querying the Kuzu database: {e.Message}";
                LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }


        /// <summary>
        /// Query N Layers
        /// </summary>
        /// <param name="projectId">The ID of the project to export from PostgreSQL into the KuzuDB</param>
        /// <param name="request">The request object containing the parameters for the query.</param>
        /// <returns>A result set containing the nodes and their relationships.</returns>
        [HttpPost("Query-N-Layers")]
        public async Task<IActionResult> GetNodesWithinDepth(int projectId, [FromBody] KuzuDBMNodesWithinDepthRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _kuzuDatabaseManager.ConnectAsync();

                await _kuzuDatabaseManager.ExportDataAsync(projectId);

                string result = await _kuzuDatabaseManager.GetNodesWithinDepthByIdAsync(request);
                return Ok(result);
            }
            catch (Exception e)
            {
                var message = $"An error occurred while retrieving nodes within depth: {e.Message}";
                LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}