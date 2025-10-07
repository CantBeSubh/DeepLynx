using deeplynx.business;
using deeplynx.helpers;
using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("/projects/{projectId}/files")]
    [NexusAuthorize]
    public class FileController : ControllerBase
        {
            private readonly FileBusiness _fileBusiness;
            private readonly ILogger<FileController> _logger;

            public FileController(FileBusiness fileBusiness, ILogger<FileController> logger)
            {
                _fileBusiness = fileBusiness;
                _logger = logger;
            }
            
            /// <summary>
            /// Upload file
            /// </summary>
            /// <param name="projectId">Id of project to which the file belongs</param>
            /// <param name="dataSourceId">Id of data source to which the file belongs</param>
            /// <param name="objectStorageId">Id of object storage method</param>
            /// <param name="file">File to upload</param>
            /// <returns></returns>
            [HttpPost("UploadFile", Name = "api_upload_file")]
            public async Task<ActionResult<RecordResponseDto>> UploadFile(
                long projectId,
                [FromQuery] long? dataSourceId,
                [FromQuery] long? objectStorageId,
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
            
            /// <summary>
            /// Update file
            /// </summary>
            /// <param name="projectId">Id of project to which the file belongs</param>
            /// <param name="recordId">Id of record that contains file info</param>
            /// <param name="file">The file to replace the old one</param>
            /// <returns></returns>
            [HttpPut("UpdateFile/{recordId}", Name = "api_update_file")]
            public async Task<ActionResult<RecordResponseDto>> UpdateFile(
                long projectId,
                long recordId,
                IFormFile file)
            {
                try
                {
                    var updatedFileInfo = await _fileBusiness
                        .UpdateFile(projectId, recordId, file);
                    return Ok(updatedFileInfo);
                }
                catch (Exception e)
                {
                    var message = $"An error occurred while updating file in record {recordId}: {e}";
                    _logger.LogError(message);
                    return StatusCode(StatusCodes.Status500InternalServerError, message);
                }
            }
            
            /// <summary>
            /// Download file
            /// </summary>
            /// <param name="projectId">Id of project to which the file belongs</param>
            /// <param name="recordId">Id of record that contains file info</param>
            [HttpGet("DownloadFile/{recordId}", Name = "api_download_file")]
            public async Task<IActionResult> DownloadFile(long projectId, long recordId)
            {
                try
                {
                    var fileStreamResult = await _fileBusiness.DownloadFile(projectId, recordId);
                    return fileStreamResult;
                }
                catch (Exception e)
                {
                    var message = $"An error occurred while downloading file in record {recordId}: {e}";
                    _logger.LogError(message);
                    return StatusCode(StatusCodes.Status500InternalServerError, message);
                }
            }
            
            
            
            /// <summary>
            /// Delete file
            /// </summary>
            /// <param name="projectId">Id of project to which the file belongs</param>
            /// <param name="recordId">Id of record that contains file info</param>
            [HttpDelete("DeleteFile/{recordId}", Name = "api_delete_file")]
            public async Task<IActionResult> DeleteFile(long projectId, long recordId)
            {
                try
                {
                    await _fileBusiness.DeleteFile(projectId, recordId);
                    return Ok(new {message = $"Deleted record {recordId} and its file"} );
                }
                catch (Exception e)
                {
                    var message = $"An error occurred while updating file in record {recordId}: {e}";
                    _logger.LogError(message);
                    return StatusCode(StatusCodes.Status500InternalServerError, message);
                }
            }
        }
}
    