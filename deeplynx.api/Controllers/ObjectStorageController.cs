using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("projects/{projectId}/storages")]
    [Authorize]
    public class ObjectStorageController : ControllerBase
    {
        private readonly IObjectStorageBusiness _objectStorageBusiness;
        private readonly ILogger<ObjectStorageController> _logger;

        public ObjectStorageController(
            IObjectStorageBusiness objectStorageBusiness, 
            ILogger<ObjectStorageController> logger
            )
        {
            _objectStorageBusiness = objectStorageBusiness;
            _logger = logger;
        }
        
        /// <summary>
        /// Get all object storages
        /// </summary>
        /// /// <param name="organizationId">The ID of the organization to which the object storages belongs</param>
        /// <param name="projectId">The ID of the project to which the object storages belong</param>
        /// <param name="hidearchived">Flag indicating whether to hide archived object storages from the result (Default true)</param>
        /// <returns></returns>
        [HttpGet("GetAllObjectStorages", Name = "api_get_all_object_storages")]
        public async Task<ActionResult<IEnumerable<ObjectStorageResponseDto>>> GetAllObjectStorages(
            long? organizationId,
            long? projectId,
            [FromQuery] bool hidearchived = true)
        {
            try
            {
                var objectStorages = 
                    await _objectStorageBusiness.GetAllObjectStorages(organizationId, projectId, hidearchived);
                
                return Ok(objectStorages);
            }
            catch (Exception ex)
            {
                var message = $"An unexpected error occurred while fetching all classes.: {ex}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Get object storage
        /// </summary>
        /// /// <param name="organizationId">The ID of the organization to which the object storage belongs</param>
        /// <param name="projectId">The ID of the project to which the object storages belong</param>
        /// <param name="hidearchived">Flag indicating whether to hide archived object storages from the result (Default true)</param>
        /// <param name="objectStorageId">ID of object storage to retrieve</param>
        /// <returns></returns>
        [HttpGet("GetObjectStorage/{objectStorageId}", Name = "api_get_object_storage")]
        public async Task<ActionResult<ObjectStorageResponseDto>> GetObjectStorage(
            long? organizationId,
            long? projectId,
            long objectStorageId,
            [FromQuery] bool hidearchived = true)
        {
            try
            {
                var objectStorage = await _objectStorageBusiness.GetObjectStorage(organizationId, projectId, objectStorageId, hidearchived);
                return Ok(objectStorage);
            }
            catch (Exception ex)
            {
                var message = $"An unexpected error occurred while fetching object storage with id {objectStorageId}: {ex}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Get default object storage
        /// </summary>
        /// /// <param name="organizationId">The ID of the organization to which the object storage belongs</param>
        /// <param name="projectId">The ID of the project to which the object storages belong</param>
        /// <returns></returns>
        [HttpGet("GetDefaultObjectStorage", Name = "api_get_default_object_storage")]
        public async Task<ActionResult<ObjectStorageResponseDto>> GetDefaultObjectStorage(long? organizationId, long? projectId)
        {
            try
            {
                var defaultObjectStorage = await _objectStorageBusiness.GetDefaultObjectStorage(organizationId, projectId);
                return Ok(defaultObjectStorage);
            }
            catch (Exception ex)
            {
                var message = $"An unexpected error occurred while fetching default object storage for project {projectId}: {ex}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Create object storage
        /// </summary>
        /// /// <param name="organizationId">The ID of the organization to which the object storage belongs</param>
        /// <param name="projectId">The ID of the project to which the object storages belong</param>
        /// <param name="dto">The dto of an object storage to be created</param>
        /// <param name="makeDefault"> Flag to indicate whether to make the created storage procedure default (Default Value = false)</param> 
        /// <returns></returns>
        [HttpPost("CreateObjectStorage", Name = "api_create_object_storage")]
        public async Task<ActionResult<ObjectStorageResponseDto>> CreateObjectStorage(
            long? organizationId,
            long? projectId,
            [FromBody] CreateObjectStorageRequestDto dto,
            [FromQuery] bool makeDefault = false)
        {
            try
            {
                var objectStorage = await _objectStorageBusiness.CreateObjectStorage(organizationId, projectId, dto, makeDefault);
                return Ok(objectStorage);
            }
            catch (Exception ex)
            {
                var message = $"An unexpected error occurred while creating this object storage: {ex}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Update object storage
        /// </summary>
        /// /// <param name="organizationId">The ID of the organization to which the object storage belongs</param>
        /// <param name="projectId">The ID of the project to which the object storages belong</param>
        /// <param name="dto">The dto of an object storage to be created</param>
        /// <param name="objectStorageId">ID of object storage to retrieve</param> 
        /// <returns></returns>
        [HttpPut("UpdateObjectStorage/{objectStorageId}", Name = "api_update_object_storage")]
        public async Task<ActionResult<ObjectStorageResponseDto>> UpdateObjectStorage(
            long? organizationId,
            long? projectId,
            long objectStorageId,
            [FromBody] UpdateObjectStorageRequestDto dto)
        {
            try
            {
                var objectStorage = await _objectStorageBusiness.UpdateObjectStorage(organizationId, projectId, objectStorageId, dto);
                return Ok(objectStorage);
            }
            catch (Exception ex)
            {
                var message = $"An unexpected error occurred while updating this object storage {objectStorageId}: {ex}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Delete object storage
        /// </summary>
        /// /// <param name="organizationId">The ID of the organization to which the object storage belongs</param>
        /// <param name="projectId">The ID of the project to which the object storages belong</param>
        /// <param name="objectStorageId">ID of object storage to delete</param> 
        /// <returns></returns>
        [HttpDelete("DeleteObjectStorage/{objectStorageId}", Name = "api_delete_object_storage")]
        public async Task<ActionResult> DeleteObjectStorage(
            long? organizationId,
            long? projectId,
            long objectStorageId)
        {
            try
            {
                await _objectStorageBusiness.DeleteObjectStorage(organizationId, projectId, objectStorageId);
                return Ok(new { message = $"Deleted object storage {objectStorageId}" });
            }
            catch (Exception ex)
            {
                var message = $"An unexpected error occurred while deleting this object storage {objectStorageId}: {ex}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Archive object storage
        /// </summary>
        /// /// <param name="organizationId">The ID of the organization to which the object storage belongs</param>
        /// <param name="projectId">The ID of the project to which the object storages belong</param>
        /// <param name="objectStorageId">ID of object storage to delete</param> 
        /// <returns></returns>
        [HttpDelete("ArchiveObjectStorage/{objectStorageId}", Name = "api_archive_object_storage")]
        public async Task<ActionResult> ArchiveObjectStorage(
            long? organizationId,
            long? projectId,
            long objectStorageId)
        {
            try
            {
                await _objectStorageBusiness.ArchiveObjectStorage(organizationId, projectId, objectStorageId);
                return Ok(new { message = $"Archived object storage {objectStorageId}" });
            }
            catch (Exception ex)
            {
                var message = $"An unexpected error occurred while archiving this object storage {objectStorageId}: {ex}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Unarchive object storage
        /// </summary>
        /// /// <param name="organizationId">The ID of the organization to which the object storage belongs</param>
        /// <param name="projectId">The ID of the project to which the object storages belong</param>
        /// <param name="objectStorageId">ID of object storage to retrieve</param> 
        /// <returns></returns>
        [HttpPut("UnarchiveObjectStorage/{objectStorageId}", Name = "api_unarchive_object_storage")]
        public async Task<ActionResult<ObjectStorageResponseDto>> UnarchiveObjectStorage(
            long? organizationId,
            long? projectId,
            long objectStorageId)
        {
            try
            {
                await _objectStorageBusiness.UnarchiveObjectStorage(organizationId, projectId, objectStorageId);
                return Ok(new { message = $"Unarchived object storage {objectStorageId}" });
            }
            catch (Exception ex)
            {
                var message = $"An unexpected error occurred while unarchiving this object storage {objectStorageId}: {ex}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Sets default object storage
        /// </summary>
        /// <param name="projectId">The ID of the project to which the object storage belongs</param>
        /// <param name="organizationId">The ID of the organization to which the object storage belongs</param>
        /// <param name="objectStorageId">ID of object storage to make default</param> 
        /// <returns></returns>
        [HttpPut("SetDefaultObjectStorage/{objectStorageId}", Name = "api_change_default_object_storage")]
        public async Task<ActionResult<ObjectStorageResponseDto>> SetDefaultObjectStorage(
            long? organizationId,
            long projectId,
            long objectStorageId)
        {
            try
            {
                await _objectStorageBusiness.SetDefaultObjectStorage(organizationId, projectId, objectStorageId);
                return Ok(new { message = $"Made object storage with id {objectStorageId} default" });
            }
            catch (Exception ex)
            {
                var message = $"An unexpected error occurred while unarchiving this object storage {objectStorageId}: {ex}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}