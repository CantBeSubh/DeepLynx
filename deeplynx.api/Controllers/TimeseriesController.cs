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
        private const string UploadFolderPath = "uploads";

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeseriesController"/> class
        /// </summary>
        /// <param name="timeseriesBusiness">The business logic interface for handling time series operations.</param>
        public TimeseriesController(ITimeseriesBusiness timeseriesBusiness)
        {
            _timeseriesBusiness = timeseriesBusiness;
        }

        
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromRoute] string projectId, [FromRoute] string dataSourceId, [FromForm] IFormFile file)
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

        [HttpPost("start-upload")]
        public IActionResult StartUpload([FromRoute] string projectId, [FromRoute] string dataSourceId, [FromBody] TimeseriesUploadInitRequestDto request)
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

        [HttpPost("upload-chunk")]
        public async Task<IActionResult> UploadChunk([FromRoute] string projectId, [FromRoute] string dataSourceId, [FromForm] IFormFile chunk, [FromForm] string uploadId, [FromForm] int chunkNumber)
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
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [HttpPost("complete-upload")]
        public async Task<IActionResult> CompleteUpload([FromRoute] string projectId, [FromRoute] string dataSourceId, [FromBody] TimeseriesUploadCompleteRequestDto request)
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