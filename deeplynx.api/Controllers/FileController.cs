using deeplynx.business;
using deeplynx.helpers.Context;
using deeplynx.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers;

/// <summary>
///     Controller for managing files.
/// </summary>
/// <remarks>
///     This controller provides endpoints to upload, update, download, and delete file information.
/// </remarks>
[ApiController]
[Route("organizations/{organizationId}/projects/{projectId}/files")]
[Authorize]
public class FileController : ControllerBase
{
    private readonly FileBusiness _fileBusiness;
    private readonly ILogger<FileController> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileController" /> class
    /// </summary>
    /// <param name="fileBusiness">The business logic interface for handling file operations.</param>
    /// <param name="logger">Error/Info logging interface for database log table.</param>
    public FileController(FileBusiness fileBusiness, ILogger<FileController> logger)
    {
        _fileBusiness = fileBusiness;
        _logger = logger;
    }

    /// <summary>
    ///     Upload a File
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the file belongs</param>
    /// <param name="dataSourceId">The ID of the data source to which the file belongs</param>
    /// <param name="objectStorageId">The ID of the object storage method</param>
    /// <param name="file">The file to upload</param>
    /// <returns>Record response DTO containing file information</returns>
    [HttpPost(Name = "api_upload_file")]
    public async Task<ActionResult<RecordResponseDto>> UploadFile(
        long organizationId,
        long projectId,
        [FromQuery] long? dataSourceId,
        [FromQuery] long? objectStorageId,
        IFormFile file)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;
            var fileUploadInfo =
                await _fileBusiness.UploadFile(currentUserId, projectId, organizationId, dataSourceId, objectStorageId,
                    file);
            return Ok(fileUploadInfo);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while uploading file {file.FileName}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Update a File
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the file belongs</param>
    /// <param name="recordId">The ID of the record that contains file information</param>
    /// <param name="file">The file to replace the old one</param>
    /// <returns>Record response DTO containing updated file information</returns>
    [HttpPut("{recordId}", Name = "api_update_file")]
    public async Task<ActionResult<RecordResponseDto>> UpdateFile(
        long organizationId,
        long projectId,
        long recordId,
        IFormFile file)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;
            var updatedFileInfo =
                await _fileBusiness.UpdateFile(currentUserId, organizationId, projectId, recordId, file);
            return Ok(updatedFileInfo);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while updating file in record {recordId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Download a File
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the file belongs</param>
    /// <param name="recordId">The ID of the record that contains file information</param>
    /// <returns>The file stream for download</returns>
    [HttpGet("{recordId}", Name = "api_download_file")]
    public async Task<IActionResult> DownloadFile(
        long organizationId,
        long projectId,
        long recordId)
    {
        try
        {
            var fileStreamResult = await _fileBusiness.DownloadFile(organizationId, projectId, recordId);
            return fileStreamResult;
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while downloading file in record {recordId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Delete a File
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the file belongs</param>
    /// <param name="recordId">The ID of the record that contains file information</param>
    /// <returns>A message stating the file was successfully deleted.</returns>
    [HttpDelete("{recordId}", Name = "api_delete_file")]
    public async Task<IActionResult> DeleteFile(
        long organizationId,
        long projectId,
        long recordId)
    {
        try
        {
            await _fileBusiness.DeleteFile(organizationId, projectId, recordId);
            return Ok(new { message = $"Deleted record {recordId} and its file" });
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while deleting file in record {recordId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }
}