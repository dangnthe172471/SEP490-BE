using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
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

        [Authorize(Roles = "Patient,Nurse,Doctor")]
        [HttpGet("{recordId:int}")]
        public async Task<ActionResult<ReadInternalMedRecordDto>> Get(
            [FromRoute] int recordId,
            CancellationToken ct = default)
        {
            var item = await _service.GetByRecordIdAsync(recordId, ct);
            if (item == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy hồ sơ khám nội cho hồ sơ khám bệnh có mã {recordId}."
                });
            }

            return Ok(item);
        }

        [Authorize(Roles = "Doctor,Nurse")]
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
                    new
                    {
                        message = "Đã xảy ra lỗi khi tạo hồ sơ khám nội.",
                        detail = ex.Message
                    });
            }
        }

        [Authorize(Roles = "Doctor,Nurse")]
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
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError,
                    new
                    {
                        message = "Đã xảy ra lỗi khi cập nhật hồ sơ khám nội.",
                        detail = ex.Message
                    });
            }
        }

        [Authorize(Roles = "Doctor,Nurse")]
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
                    new
                    {
                        message = "Đã xảy ra lỗi khi xóa hồ sơ khám nội.",
                        detail = ex.Message
                    });
            }
        }

        [Authorize(Roles = "Patient,Nurse,Doctor")]
        [HttpGet("status/{recordId:int}")]
        public async Task<ActionResult<object>> GetSpecialtyStatus(
            [FromRoute] int recordId,
            CancellationToken ct = default)
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
