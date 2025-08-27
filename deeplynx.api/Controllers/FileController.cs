using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/files")]
    public class FileController : ControllerBase
        {
            private readonly IFileBusiness _fileBusiness;
            private readonly ILogger<FileController> _logger;

            public FileController(IFileBusiness fileBusiness, ILogger<FileController> logger)
            {
                _fileBusiness = fileBusiness;
                _logger = logger;
            }

            [HttpPost("UploadFile")]
            public async Task<ActionResult<RecordResponseDto>> UploadFile(
                long projectId,
                [FromQuery] long dataSourceId,
                [FromQuery] long objectStorageId,
                IFormFile file)
            {
                try
                {
                    var fileUploadInfo = await _fileBusiness
                        .UploadFile(projectId, dataSourceId, objectStorageId, file);
                    return Ok(fileUploadInfo);
                }
                catch (Exception e)
                {
                    var message = $"An error occurred while uploading file {file.FileName}: {e}";
                    _logger.LogError(message);
                    return StatusCode(StatusCodes.Status500InternalServerError, message);
                }
            }
            // TODO: Upload File
            // TODO: Update File (upload a newer copy)
            // TODO: Download File
            // TODO: Delete File
        }
}
    