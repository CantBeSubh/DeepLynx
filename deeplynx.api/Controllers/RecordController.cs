using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using deeplynx.datalayer;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    public class RecordController : ControllerBase
    {
        private readonly IRecordBusiness _recordService;

        public RecordController(IRecordBusiness recordService)
        {
            _recordService = recordService;
        }
        [HttpGet("projects/{projectId}/datasources/{dataSourceId}/records")]
        public async Task<IActionResult> GetAllRecords(long projectId, long dataSourceId)
        {
            var records = await _recordService.GetAllRecords(projectId, dataSourceId);
            return Ok(records);
        }
        [HttpGet("projects/{projectId}/datasources/{dataSourceId}/records/{recordId}")]
        public async Task<IActionResult> GetRecord(long projectId, long dataSourceId, long recordId)
        {
            var record = await _recordService.GetRecord(projectId, dataSourceId, recordId);
            return Ok(record);
        }

        [HttpPost("projects/{projectId}/datasources/{dataSourceId}/records")]
        public async Task<IActionResult> CreateRecord(long projectId, long dataSourceId,
            [FromBody] RecordRequestDto dto)
        {
            var record = await _recordService.CreateRecord(projectId, dataSourceId, dto);
            return Ok(record);
            // return CreatedAtAction(nameof(GetRecord), new { projectId, dataSourceId, recordId = record.Id }, record);
        }

        [HttpPut("projects/{projectId}/datasources/{dataSourceId}/records/{recordId}")]
        public async Task<IActionResult> UpdateRecord(long projectId, long dataSourceId, long recordId,
            [FromBody] RecordRequestDto dto)
        {
            var updated = await _recordService.UpdateRecord(projectId, dataSourceId, recordId, dto);
            return Ok(updated);
        }

        [HttpDelete("projects/{projectId}/datasources/{dataSourceId}/records/{recordId}")]
        public async Task<IActionResult> DeleteRecord(long projectId, long dataSourceId, long recordId)
        {
            await _recordService.DeleteRecord(projectId, dataSourceId, recordId);
            return NoContent();
        }
    }
}

