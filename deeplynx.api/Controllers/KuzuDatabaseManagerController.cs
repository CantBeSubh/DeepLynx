using deeplynx.helpers;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using Microsoft.AspNetCore.Authorization;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/graph")]
    [Authorize]
    public class KuzuDatabaseManagerController : ControllerBase
    {
        private readonly IKuzuDatabaseManager _kuzuDatabaseManager;
        private readonly ILogger<KuzuDatabaseManagerController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="KuzuDatabaseManagerController"/> class
        /// </summary>
        /// <param name="kuzuDatabaseManager">The business logic interface for handling kuzu database operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public KuzuDatabaseManagerController(IKuzuDatabaseManager kuzuDatabaseManager, ILogger<KuzuDatabaseManagerController> logger)
        {
            _kuzuDatabaseManager = kuzuDatabaseManager;
            _logger = logger;
        }


        /// <summary>
        /// Export data
        /// </summary>
        /// <param name="projectId">The ID of the project to export from PostgreSQL into the KuzuDB</param>
        /// <returns>A status indicating the success or failure of the export operation.</returns>
        [HttpPost("Export", Name = "api_export_kuzu")]
        public async Task<IActionResult> ExportData(int projectId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _kuzuDatabaseManager.ConnectAsync();

                bool exportSuccess = await _kuzuDatabaseManager.ExportDataAsync(projectId);
                if (!exportSuccess)
                {
                    return Ok("Data export completed successfully.");
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Data export failed.");
                }
            }
            catch (Exception e)
            {
                var message = $"An error occurred while exporting data to the Kuzu database: {e.Message}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }


        /// <summary>
        /// Query kuzu
        /// </summary>
        /// <param name="projectId">The ID of the project to export from PostgreSQL into the KuzuDB</param>
        /// <param name="request">The SQL query string to execute</param>
        /// <returns>A result set from the Kuzu database</returns>
        [HttpPost("Query", Name = "api_query_kuzu")]
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

                (string formattedString, object[] results) = await _kuzuDatabaseManager.ExecuteQueryAsync(request);

                if (request.Query.Contains("CALL"))
                {
                    return Ok(formattedString);
                }

                return Ok(results);
            }
            catch (Exception e)
            {
                var message = $"An error occurred while querying the Kuzu database: {e.Message}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }


        /// <summary>
        /// Query n layers
        /// </summary>
        /// <param name="projectId">The ID of the project to export from PostgreSQL into the KuzuDB</param>
        /// <param name="request">The request object containing the parameters for the query.</param>
        /// <returns>A result set containing the nodes and their relationships.</returns>
        [HttpPost("Query-N-Layers", Name = "api_query_n_layers")]
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

                (object[]? results, string formattedString) = await _kuzuDatabaseManager.GetNodesWithinDepthByIdAsync(request);

                if (results != null)
                {
                    return Ok(results);
                }
                else
                {
                    return Ok(formattedString);
                }
            }
            catch (Exception e)
            {
                var message = $"An error occurred while retrieving nodes within depth: {e.Message}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}