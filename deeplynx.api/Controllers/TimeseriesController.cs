using deeplynx.helpers.exceptions;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/datasources/{dataSourceId}/timeseries")]
    public class TimeseriesController : ControllerBase
    {
        private readonly ITimeseriesBusiness _timeseriesBusiness;
        private readonly ILogger<TimeseriesController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeseriesController"/> class
        /// </summary>
        /// <param name="timeseriesBusiness">The business logic interface for handling time series operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public TimeseriesController(ITimeseriesBusiness timeseriesBusiness, ILogger<TimeseriesController> logger)
        {
            _timeseriesBusiness = timeseriesBusiness;
            _logger = logger;
        }

        /// <summary>
        /// Query timeseries 
        /// </summary>
        /// <param name="request"> The request containing an sql query string</param>
        /// <param name="projectId">ID of project that timeseries data is associated with</param>
        /// <param name="dataSourceId">ID of data source that timeseries data is associated with</param>
        /// <returns></returns>
        [HttpPost("Query", Name = "api_query_timeseries")]
        public async Task<ActionResult<RecordResponseDto>> QueryTimeseries(long projectId, long dataSourceId, [FromBody] TimeseriesQueryRequestDto request)
        {
            try
            {
                var reportRecordResponse = await _timeseriesBusiness.QueryTimeseries(request, projectId, dataSourceId);
                return Ok(reportRecordResponse);
            }
            catch (NoResultsException nrException)
            {
                return Ok(nrException.Message);
            }
            catch (Exception e)
            {
                var message = $"An error occurred while querying timeseries table {e}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Upload timeseries file 
        /// </summary>
        /// <param name="projectId">ID of project that timeseries data is associated with</param>
        /// <param name="dataSourceId">ID of data source that timeseries data is associated with</param>
        /// <param name="file">Timeseries file</param>
        /// <returns>Record response DTO</returns>
        [HttpPost("upload", Name = "api_upload_timeseries_file")]
        public async Task<ActionResult<RecordResponseDto>> UploadFile(long projectId, long dataSourceId, IFormFile file)
        {
            try
            {
                var timeSeriesUploadInfo = await _timeseriesBusiness.UploadFile(projectId, dataSourceId, file);
                return Ok(timeSeriesUploadInfo);
            }
            catch (Exception e)
            {
                var message = $"An error occurred while uploading timeseries file {file.FileName}: {e}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Start timeseries upload
        /// </summary>
        /// <param name="projectId">ID of project that timeseries data is associated with</param>
        /// <param name="dataSourceId">ID of data source that timeseries data is associated with</param>
        /// <param name="request">Timeseries request DTO</param>
        /// <returns>{UploadId}</returns>
        [HttpPost("start-upload", Name = "api_start_timeseries_upload")]
        public async Task<IActionResult> StartUpload(long projectId, long dataSourceId, [FromBody] TimeseriesUploadInitRequestDto request)
        {
            try
            {
                var uploadId = await _timeseriesBusiness.StartUpload(projectId, dataSourceId);
                return Ok(new { UploadId = uploadId });
            }
            catch (Exception e)
            {
                var message = $"An error occurred while starting an upload for timeseries file {request.FileName}: {e}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Upload timeseries chunk 
        /// </summary>
        /// <param name="projectId">ID of project that timeseries data is associated with</param>
        /// <param name="dataSourceId">ID of data source that timeseries data is associated with</param>
        /// <param name="chunk">Chunk from form</param>
        /// <param name="uploadId">ID of upload</param>
        /// <param name="chunkNumber">Chunk number from form</param>
        /// <returns>{ChunkUploadStatus}</returns>
        [HttpPost("upload-chunk", Name = "api_upload_timeseries_chunk")]
        public async Task<IActionResult> UploadChunk(long projectId, long dataSourceId, IFormFile chunk, [FromForm] string uploadId, [FromForm] int chunkNumber)
        {
            try
            {
                var chunkUploadStatus =
                    await _timeseriesBusiness.UploadChunk(projectId, dataSourceId, chunk, uploadId, chunkNumber);
                return Ok(new { ChunkUploadStatus = chunkUploadStatus });
            }
            catch (Exception e)
            {
                var message = $"An error occurred while uploading a chunk for timeseries file {uploadId}: {e}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Complete timeseries upload 
        /// </summary>
        /// <param name="projectId">ID of project that timeseries data is associated with</param>
        /// <param name="dataSourceId">ID of data source that timeseries data is associated with</param>
        /// <param name="request">Timeseries request DTO</param>
        /// <returns>{TimeseriesUploadRecord}</returns>
        [HttpPost("complete-upload", Name = "api_complete_timeseries_upload")]
        public async Task<ActionResult<RecordResponseDto>> CompleteUpload(long projectId, long dataSourceId, [FromBody] TimeseriesUploadCompleteRequestDto request)
        {
            try
            {
                var timeseriesUploadRecord = await _timeseriesBusiness.CompleteUpload(projectId, dataSourceId, request);
                return Ok(new { TimeseriesUploadRecord = timeseriesUploadRecord });
            }
            catch (Exception e)
            {
                var message = $"An error occurred while completing a timeseries file upload for {request.FileName}: {e}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        [HttpPatch("append", Name = "api_append_timeseries_file")]
        public async Task<ActionResult<string>> AppendFile(long projectId, long dataSourceId, IFormFile file)
        {
            try
            {
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    var fileContent = await reader.ReadToEndAsync();
                    Console.WriteLine(fileContent); // Print the file content
                }
                return Ok("Data Received 👍");
            }
            catch (Exception e)
            {
                var message = $"An error occurred while completing a timeseries file upload for {file.FileName}: {e}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Get every nth timeseries table row
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="dataSourceId"></param>
        /// <param name="tableName"></param>
        /// <param name="rowNumber"></param>
        /// <returns></returns>
        [HttpGet("InterpolateRows", Name = "api_interpolate_timeseries_rows")]
        public async Task<IActionResult> InterpolateRows(long projectId, long dataSourceId, [FromQuery] string tableName, [FromQuery] string rowNumber)
        {
            try
            {
                var timeseriesUploadRecord = await _timeseriesBusiness.InterpolateRows(projectId, dataSourceId, rowNumber, tableName);
                return Ok(new { TimeseriesUploadRecord = timeseriesUploadRecord });
            }
            catch (Exception e)
            {
                var message = $"An error occurred while querying a timeseries table {tableName}: {e}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        /// <summary>
        /// Get all timeseries table rows
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="projectId"></param>
        /// <param name="dataSourceId"></param>
        /// <returns></returns>
        [HttpGet("GetAll", Name = "api_get_all_timeseries_records")]
        public async Task<IActionResult> GetAllTableRecords(long projectId, long dataSourceId, [FromQuery] string tableName)
        {
            try
            {
                var timeseriesUploadRecord = await _timeseriesBusiness.GetAllTableRecords(projectId, dataSourceId, tableName);
                return Ok(new { TimeseriesUploadRecord = timeseriesUploadRecord });
            }
            catch (Exception e)
            {
                var message = $"An error occurred while querying a timeseries table {tableName}: {e}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }
    }
}