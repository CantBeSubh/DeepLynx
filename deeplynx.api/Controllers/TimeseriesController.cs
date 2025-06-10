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

        [HttpPost("start-upload")]
        public IActionResult StartUpload([FromRoute] string projectId, [FromRoute] string dataSourceId, [FromBody] TimeseriesUploadInitRequestDto request)
        {
            var uploadId = Guid.NewGuid().ToString();
            var folderPath = Path.Combine(UploadFolderPath, uploadId);
            Directory.CreateDirectory(folderPath);

            // store some metadata about the upload session?

            return Ok(new { UploadId = uploadId });
        }

        [HttpPost("upload-chunk")]
        public async Task<IActionResult> UploadChunk([FromRoute] string projectId, [FromRoute] string dataSourceId, [FromForm] IFormFile chunk, [FromForm] string uploadId, [FromForm] int chunkNumber)
        {
            if (chunk == null || chunk.Length == 0)
            {
                return BadRequest("No chunk uploaded.");
            }

            var tempFilePath = Path.Combine(UploadFolderPath, uploadId, $"{chunkNumber}.part");
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await chunk.CopyToAsync(stream);
            }

            return Ok(new { Message = "Chunk uploaded successfully." });
        }

        [HttpPost("complete-upload")]
        public async Task<IActionResult> CompleteUpload([FromRoute] string projectId, [FromRoute] string dataSourceId, [FromBody] TimeseriesUploadCompleteRequestDto request)
        {
            var folderPath = Path.Combine(UploadFolderPath, request.UploadId);
            var finalFilePath = Path.Combine(UploadFolderPath, request.FileName);

            using (var finalFileStream = new FileStream(finalFilePath, FileMode.Create))
            {
                for (int i = 0; i < request.TotalChunks; i++)
                {
                    var partFilePath = Path.Combine(folderPath, $"{i}.part");
                    using (var partStream = new FileStream(partFilePath, FileMode.Open))
                    {
                        await partStream.CopyToAsync(finalFileStream);
                    }
                    System.IO.File.Delete(partFilePath); // Clean up the chunk file
                }
            }

            Directory.Delete(folderPath); // Clean up the upload folder

            var timeSeriesDataDTO = new TimeseriesDataDto
            {
                ProjectId = projectId,
                DataSourceId = dataSourceId,
                FileName = request.FileName,
                FilePath = finalFilePath,
                FileType = Path.GetExtension(request.FileName).TrimStart('.').ToLower()
            };

            // todo: something like `await _timeseriesBusiness.ProcessTimeSeriesDataAsync(timeSeriesDataDTO);`
            await _timeseriesBusiness.ProcessTimeSeriesDataAsync(timeSeriesDataDTO);

            return Ok(new { Message = "Upload completed successfully." });
        }
    }
}