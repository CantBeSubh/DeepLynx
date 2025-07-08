using deeplynx.helpers.exceptions;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/datasources/{dataSourceId}/timeseries")]
    public class TimeseriesController : ControllerBase
    {
        private readonly ITimeseriesBusiness _timeseriesBusiness;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeseriesController"/> class
        /// </summary>
        /// <param name="timeseriesBusiness">The business logic interface for handling time series operations.</param>
        public TimeseriesController(ITimeseriesBusiness timeseriesBusiness)
        {
            _timeseriesBusiness = timeseriesBusiness;
        }

        /// <summary>
        /// Query timeseries 
        /// </summary>
        /// <param name="request"> The request containing an sql query string</param>
        /// <param name="projectId">ID of project that timeseries data is associated with</param>
        /// <param name="dataSourceId">ID of data source that timeseries data is associated with</param>
        /// <returns></returns>
        [HttpPost("query")]
        public async Task<IActionResult> QueryTimeseries(string projectId, string dataSourceId, [FromBody] TimeseriesQueryRequestDto request)
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
                LogManager.GetCurrentClassLogger().Error(message);
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
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(string projectId, string dataSourceId, IFormFile file)
        {
            try
            {
                var timeSeriesUploadInfo = await _timeseriesBusiness.UploadFile(projectId, dataSourceId, file);
                return Ok(timeSeriesUploadInfo);
            }
            catch (Exception e)
            {
                var message = $"An error occurred while uploading timeseries file {file.FileName}: {e}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
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
        [HttpPost("start-upload")]
        public IActionResult StartUpload(string projectId, string dataSourceId, [FromBody] TimeseriesUploadInitRequestDto request)
        {
            try
            {
                var uploadId = _timeseriesBusiness.StartUpload(projectId, dataSourceId);
                return Ok(new { UploadId = uploadId });
            }
            catch (Exception e)
            {
                var message = $"An error occurred while starting an upload for timeseries file {request.FileName}: {e}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
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
        [HttpPost("upload-chunk")]
        public async Task<IActionResult> UploadChunk(string projectId, string dataSourceId, IFormFile chunk, [FromForm] string uploadId, [FromForm] int chunkNumber)
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
                LogManager.GetCurrentClassLogger().Error(message);
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
        [HttpPost("complete-upload")]
        public async Task<IActionResult> CompleteUpload(string projectId, string dataSourceId, [FromBody] TimeseriesUploadCompleteRequestDto request)
        {
            try
            {
                var timeseriesUploadRecord = await _timeseriesBusiness.CompleteUpload(projectId, dataSourceId, request);
                return Ok(new { TimeseriesUploadRecord = timeseriesUploadRecord });
            }
            catch (Exception e)
            {
                var message = $"An error occurred while completing a timeseries file upload for {request.FileName}: {e}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
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
        [HttpGet("InterpolateRows")]
        public async Task<IActionResult> InterpolateRows(string projectId, string dataSourceId, [FromQuery] string tableName, [FromQuery] string rowNumber)
        {
            try
            {
                var timeseriesUploadRecord = await _timeseriesBusiness.InterpolateRows(projectId, dataSourceId, rowNumber, tableName);
                return Ok(new { TimeseriesUploadRecord = timeseriesUploadRecord });
            }
            catch (Exception e)
            {
                var message = $"An error occurred while querying a timeseries table {tableName}: {e}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
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
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllTableRecords([FromQuery] string tableName, string projectId, string dataSourceId)
        {
            try
            {
                var timeseriesUploadRecord = await _timeseriesBusiness.GetAllTableRecords(tableName, projectId, dataSourceId);
                return Ok(new { TimeseriesUploadRecord = timeseriesUploadRecord });
            }
            catch (Exception e)
            {
                var message = $"An error occurred while querying a timeseries table {tableName}: {e}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }
    }
}