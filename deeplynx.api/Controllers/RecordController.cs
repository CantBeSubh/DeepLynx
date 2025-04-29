using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    public class RecordController : ControllerBase
    {
        private readonly IRecordBusiness _recordBusiness;

        public RecordController(IRecordBusiness recordBusiness)
        {
            _recordBusiness = recordBusiness;
        }
        [HttpGet("projects/{projectId}/datasources/{dataSourceId}/records")]
        public async Task<IActionResult> GetAllRecords(long projectId, long dataSourceId)
        {
            var records = await _recordBusiness.GetAllRecords(projectId, dataSourceId);
            return Ok(records);
        }
        [HttpGet("projects/{projectId}/datasources/{dataSourceId}/records/{recordId}")]
        public async Task<IActionResult> GetRecord(long projectId, long dataSourceId, long recordId)
        {
            var record = await _recordBusiness.GetRecord(projectId, dataSourceId, recordId);
            return Ok(record);
        }

        [HttpPost("projects/{projectId}/datasources/{dataSourceId}/records")]
        public async Task<IActionResult> CreateRecord(long projectId, long dataSourceId,
            [FromBody] RecordRequestDto dto)
        {
            var record = await _recordBusiness.CreateRecord(projectId, dataSourceId, dto);
            return Ok(record);
        }

        [HttpPut("projects/{projectId}/datasources/{dataSourceId}/records/{recordId}")]
        public async Task<IActionResult> UpdateRecord(long projectId, long dataSourceId, long recordId,
            [FromBody] RecordRequestDto dto)
        {
            var updated = await _recordBusiness.UpdateRecord(projectId, dataSourceId, recordId, dto);
            return Ok(updated);
        }

        [HttpDelete("projects/{projectId}/datasources/{dataSourceId}/records/{recordId}")]
        public async Task<IActionResult> DeleteRecord(long projectId, long dataSourceId, long recordId)
        {
            await _recordBusiness.DeleteRecord(projectId, dataSourceId, recordId);
            return NoContent();
        }
    }
}

