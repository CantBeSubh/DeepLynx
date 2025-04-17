using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using deeplynx.datalayer;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("projects/{projectId}/datasources/{dataSourceId}/records")]
    public class RecordController : ControllerBase
    {
        private readonly IRecordBusiness _recordService;

        public RecordController(IRecordBusiness recordService)
        {
            _recordService = recordService;
        }

        [HttpGet("{recordId}")]
        public async Task<IActionResult> GetRecord(long projectId, long dataSourceId, long recordId)
        {
            var record = await _recordService.GetRecord(projectId, dataSourceId, recordId);
            return Ok(record);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecord(long projectId, long dataSourceId,
            [FromBody] RecordRequestDto dto)
        {
            var record = await _recordService.CreateRecord(projectId, dataSourceId, dto);
            return CreatedAtAction(nameof(GetRecord), new { projectId, dataSourceId, recordId = record.Id }, record);
        }

        [HttpPut("{recordId}")]
        public async Task<IActionResult> UpdateRecord(long projectId, long dataSourceId, long recordId,
            [FromBody] RecordRequestDto dto)
        {
            var updated = await _recordService.UpdateRecord(projectId, dataSourceId, recordId, dto);
            return Ok(updated);
        }

        [HttpDelete("{recordId}")]
        public async Task<IActionResult> DeleteRecord(long projectId, long dataSourceId, long recordId)
        {
            await _recordService.DeleteRecord(projectId, dataSourceId, recordId);
            return NoContent();
        }
    }
}

