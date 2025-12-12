using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.TestReDTO;
using System.Net;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestResultsController : ControllerBase
    {
        private readonly ITestResultService _service;
        public TestResultsController(ITestResultService service) => _service = service;

        [Authorize(Roles = "Nurse")]
        [HttpGet("worklist")]
        public async Task<ActionResult<PagedResult<TestWorklistItemDto>>> GetWorklist(
            [FromQuery] string? date,
            [FromQuery] string? patientName,
            [FromQuery] string requiredState = "All",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            try
            {
                if (!Enum.TryParse<RequiredState>(requiredState, true, out var state))
                    state = RequiredState.All;

                if (!string.IsNullOrWhiteSpace(patientName) &&
                    string.Equals(patientName, "patientName", StringComparison.OrdinalIgnoreCase))
                {
                    patientName = null;
                }

                DateOnly? visitDate = null;
                if (!string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var d))
                {
                    visitDate = d;
                }

                var q = new TestWorklistQueryDto
                {
                    VisitDate = visitDate,
                    PatientName = patientName,
                    RequiredState = state,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _service.GetWorklistAsync(q, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError,
                    new
                    {
                        message = "Đã xảy ra lỗi hệ thống khi lấy danh sách worklist xét nghiệm.",
                        detail = ex.Message
                    });
            }
        }

        [Authorize(Roles = "Doctor,Patient,Nurse,Receptionist")]
        [HttpGet("record/{recordId:int}")]
        public async Task<ActionResult<List<ReadTestResultDto>>> GetByRecordId(
            [FromRoute] int recordId,
            CancellationToken ct = default)
        {
            try
            {
                var items = await _service.GetByRecordIdAsync(recordId, ct);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError,
                    new
                    {
                        message = "Đã xảy ra lỗi hệ thống khi lấy danh sách kết quả xét nghiệm theo phiếu khám.",
                        detail = ex.Message
                    });
            }
        }

        [Authorize(Roles = "Doctor,Patient,Nurse,Receptionist")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReadTestResultDto>> GetById(
            [FromRoute] int id,
            CancellationToken ct = default)
        {
            try
            {
                var item = await _service.GetByIdAsync(id, ct);
                if (item == null)
                {
                    return NotFound(new
                    {
                        message = $"Không tìm thấy kết quả xét nghiệm với mã {id}."
                    });
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError,
                    new
                    {
                        message = "Đã xảy ra lỗi hệ thống khi lấy chi tiết kết quả xét nghiệm.",
                        detail = ex.Message
                    });
            }
        }

        [Authorize(Roles = "Doctor,Nurse")]
        [HttpPost]
        public async Task<ActionResult<ReadTestResultDto>> Create(
            [FromBody] CreateTestResultDto dto,
            CancellationToken ct = default)
        {
            try
            {
                var created = await _service.CreateAsync(dto, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.TestResultId }, created);
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
                        message = "Đã xảy ra lỗi hệ thống khi tạo kết quả xét nghiệm.",
                        detail = ex.Message
                    });
            }
        }

        [Authorize(Roles = "Doctor,Nurse")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ReadTestResultDto>> Update(
            [FromRoute] int id,
            [FromBody] UpdateTestResultDto dto,
            CancellationToken ct = default)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto, ct);
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
                        message = "Đã xảy ra lỗi hệ thống khi cập nhật kết quả xét nghiệm.",
                        detail = ex.Message
                    });
            }
        }

        [Authorize(Roles = "Doctor,Nurse")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(
            [FromRoute] int id,
            CancellationToken ct = default)
        {
            try
            {
                await _service.DeleteAsync(id, ct);
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
                        message = "Đã xảy ra lỗi hệ thống khi xoá kết quả xét nghiệm.",
                        detail = ex.Message
                    });
            }
        }

        [Authorize(Roles = "Doctor,Nurse")]
        [HttpGet("types")]
        public async Task<ActionResult<List<TestTypeLite>>> GetTypes(CancellationToken ct = default)
        {
            try
            {
                var types = await _service.GetTestTypesAsync(ct);
                return Ok(types);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError,
                    new
                    {
                        message = "Đã xảy ra lỗi hệ thống khi lấy danh sách loại xét nghiệm.",
                        detail = ex.Message
                    });
            }
        }
    }
}
