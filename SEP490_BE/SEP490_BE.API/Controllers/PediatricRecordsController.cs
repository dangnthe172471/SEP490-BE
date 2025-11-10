using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.PediatricRecordsDTO;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PediatricRecordsController : ControllerBase
    {
        private readonly IPediatricRecordService _service;
        public PediatricRecordsController(IPediatricRecordService service) => _service = service;

        [HttpGet("{recordId:int}")]
        public async Task<ActionResult<ReadPediatricRecordDto>> Get([FromRoute] int recordId, CancellationToken ct)
        {
            var item = await _service.GetByRecordIdAsync(recordId, ct);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<ReadPediatricRecordDto>> Create([FromBody] CreatePediatricRecordDto dto, CancellationToken ct)
        {
            var created = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(Get), new { recordId = created.RecordId }, created);
        }

        [HttpPut("{recordId:int}")]
        public async Task<ActionResult<ReadPediatricRecordDto>> Update([FromRoute] int recordId, [FromBody] UpdatePediatricRecordDto dto, CancellationToken ct)
        {
            var updated = await _service.UpdateAsync(recordId, dto, ct);
            return Ok(updated);
        }

        [HttpDelete("{recordId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int recordId, CancellationToken ct)
        {
            await _service.DeleteAsync(recordId, ct);
            return NoContent();
        }
    }
}
