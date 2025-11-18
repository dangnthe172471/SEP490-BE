using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.BLL.Services;
using SEP490_BE.DAL.DTOs.InternalMedRecordsDTO;
using System.Net;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InternalMedRecordsController : ControllerBase
    {
        private readonly IInternalMedRecordService _service;

        public InternalMedRecordsController(IInternalMedRecordService service)
        {
            _service = service;
        }

        [HttpGet("{recordId:int}")]
        public async Task<ActionResult<ReadInternalMedRecordDto>> Get(
            [FromRoute] int recordId,
            CancellationToken ct = default)
        {
            var item = await _service.GetByRecordIdAsync(recordId, ct);
            if (item == null) return NotFound(new { message = $"InternalMedRecord cho RecordId {recordId} không tồn tại" });
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<ReadInternalMedRecordDto>> Create(
            [FromBody] CreateInternalMedRecordDto dto,
            CancellationToken ct = default)
        {
            try
            {
                var created = await _service.CreateAsync(dto, ct);
                return CreatedAtAction(nameof(Get), new { recordId = created.RecordId }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError,
                    new { message = "Lỗi khi tạo InternalMedRecord", detail = ex.Message });
            }
        }

        [HttpPut("{recordId:int}")]
        public async Task<ActionResult<ReadInternalMedRecordDto>> Update(
            [FromRoute] int recordId,
            [FromBody] UpdateInternalMedRecordDto dto,
            CancellationToken ct = default)
        {
            try
            {
                var updated = await _service.UpdateAsync(recordId, dto, ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError,
                    new { message = "Lỗi khi cập nhật InternalMedRecord", detail = ex.Message });
            }
        }

        [HttpDelete("{recordId:int}")]
        public async Task<IActionResult> Delete(
            [FromRoute] int recordId,
            CancellationToken ct = default)
        {
            try
            {
                await _service.DeleteAsync(recordId, ct);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError,
                    new { message = "Lỗi khi xóa InternalMedRecord", detail = ex.Message });
            }
        }

        [HttpGet("status/{recordId:int}")]
        public async Task<ActionResult<object>> GetSpecialtyStatus([FromRoute] int recordId, CancellationToken ct = default)
        {
            try
            {
                var (hasPedia, hasInternal, hasDerm) = await _service.CheckSpecialtiesAsync(recordId, ct);

                return Ok(new
                {
                    RecordId = recordId,
                    HasPediatric = hasPedia,
                    HasInternalMed = hasInternal,
                    HasDermatology = hasDerm,
                    HasAny = hasPedia || hasInternal || hasDerm
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


    }
}
