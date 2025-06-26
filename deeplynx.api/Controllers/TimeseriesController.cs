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
        private const string UploadFolderPath = "uploads";

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
        /// <param name="projectId"></param>
        /// <param name="dataSourceId"></param>
        /// <returns></returns>
        [HttpPost("query")]
        public async Task<IActionResult> QueryTimeseries(string projectId, string dataSourceId, [FromBody]TimeseriesQueryRequestDto request)
        {
            try
            {
                var timeseriesDataTable = await _timeseriesBusiness.QueryTimeseries(request, projectId, dataSourceId);
                return Ok(timeseriesDataTable);
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
        /// <param name="projectId"></param>
        /// <param name="dataSourceId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(string projectId, string dataSourceId, IFormFile file)
        {
            try
            {
                var timeSeriesUploadInfo= await _timeseriesBusiness.UploadFile(projectId, dataSourceId, file);
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
        /// <param name="projectId"></param>
        /// <param name="dataSourceId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
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
        /// <param name="projectId"></param>
        /// <param name="dataSourceId"></param>
        /// <param name="chunk"></param>
        /// <param name="uploadId"></param>
        /// <param name="chunkNumber"></param>
        /// <returns></returns>
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
        /// Comeplete timeseries upload 
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="dataSourceId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
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
    }
}