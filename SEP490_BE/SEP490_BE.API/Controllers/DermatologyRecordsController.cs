using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.DermatologyDTO;
using System.Net;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DermatologyRecordsController : ControllerBase
    {
        private readonly IDermatologyRecordService _service;

        public DermatologyRecordsController(IDermatologyRecordService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Patient,Nurse,Doctor")]
        [HttpGet("{recordId:int}")]
        public async Task<ActionResult<ReadDermatologyRecordDto>> Get(
            [FromRoute] int recordId,
            CancellationToken ct = default)
        {
            var item = await _service.GetByRecordIdAsync(recordId, ct);
            if (item == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy hồ sơ da liễu cho phiếu khám có mã {recordId}."
                });
            }

            return Ok(item);
        }

        [Authorize(Roles = "Doctor,Nurse")]
        [HttpPost]
        public async Task<ActionResult<ReadDermatologyRecordDto>> Create(
            [FromBody] CreateDermatologyRecordDto dto,
            CancellationToken ct = default)
        {
            try
            {
                var created = await _service.CreateAsync(dto, ct);
                return CreatedAtAction(nameof(Get), new { recordId = created.RecordId }, created);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
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
                        message = "Đã xảy ra lỗi hệ thống khi tạo hồ sơ da liễu.",
                        detail = ex.Message
                    });
            }
        }

        [Authorize(Roles = "Doctor,Nurse")]
        [HttpPut("{recordId:int}")]
        public async Task<ActionResult<ReadDermatologyRecordDto>> Update(
            [FromRoute] int recordId,
            [FromBody] UpdateDermatologyRecordDto dto,
            CancellationToken ct = default)
        {
            try
            {
                var updated = await _service.UpdateAsync(recordId, dto, ct);
                return Ok(updated);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError,
                    new
                    {
                        message = "Đã xảy ra lỗi hệ thống khi cập nhật hồ sơ da liễu.",
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
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError,
                    new
                    {
                        message = "Đã xảy ra lỗi hệ thống khi xoá hồ sơ da liễu.",
                        detail = ex.Message
                    });
            }
        }
    }
}
