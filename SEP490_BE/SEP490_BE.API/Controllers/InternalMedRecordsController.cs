using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.BLL.Services;
using SEP490_BE.DAL.DTOs.InternalMedRecordsDTO;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InternalMedRecordsController : ControllerBase
    {
        private readonly IInternalMedRecordService _service;
        public InternalMedRecordsController(IInternalMedRecordService service) => _service = service;

        [HttpGet("{recordId:int}")]
        public async Task<ActionResult<ReadInternalMedRecordDto>> Get([FromRoute] int recordId, CancellationToken ct)
        {
            var item = await _service.GetByRecordIdAsync(recordId, ct);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<ReadInternalMedRecordDto>> Create([FromBody] CreateInternalMedRecordDto dto, CancellationToken ct)
        {
            var created = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(Get), new { recordId = created.RecordId }, created);
        }

        [HttpPut("{recordId:int}")]
        public async Task<ActionResult<ReadInternalMedRecordDto>> Update([FromRoute] int recordId, [FromBody] UpdateInternalMedRecordDto dto, CancellationToken ct)
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

        [HttpGet("status/{recordId:int}")]
        public async Task<ActionResult<object>> GetSpecialtyStatus([FromRoute] int recordId, CancellationToken ct)
        {
            var (hasPedia, hasInternal) = await _service.CheckSpecialtiesAsync(recordId, ct);
            return Ok(new
            {
                RecordId = recordId,
                HasPediatric = hasPedia,
                HasInternalMed = hasInternal,
                HasBoth = hasPedia && hasInternal
            });
        }
    }
}
