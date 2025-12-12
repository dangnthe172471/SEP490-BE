using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.TestReDTO;

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

        [Authorize(Roles = "Doctor,Patient,Nurse,Receptionist")]
        [HttpGet("record/{recordId:int}")]
        public async Task<ActionResult<List<ReadTestResultDto>>> GetByRecordId(
            [FromRoute] int recordId,
            CancellationToken ct = default)
        {
            var items = await _service.GetByRecordIdAsync(recordId, ct);
            return Ok(items);
        }

        [Authorize(Roles = "Doctor,Patient,Nurse,Receptionist")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReadTestResultDto>> GetById(
            [FromRoute] int id,
            CancellationToken ct = default)
        {
            var item = await _service.GetByIdAsync(id, ct);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [Authorize(Roles = "Doctor,Nurse")]
        [HttpPost]
        public async Task<ActionResult<ReadTestResultDto>> Create(
            [FromBody] CreateTestResultDto dto,
            CancellationToken ct = default)
        {
            var created = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.TestResultId }, created);
        }

        [Authorize(Roles = "Doctor,Nurse")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ReadTestResultDto>> Update(
            [FromRoute] int id,
            [FromBody] UpdateTestResultDto dto,
            CancellationToken ct = default)
        {
            var updated = await _service.UpdateAsync(id, dto, ct);
            return Ok(updated);
        }

        [Authorize(Roles = "Doctor,Nurse")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(
            [FromRoute] int id,
            CancellationToken ct = default)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        [Authorize(Roles = "Doctor,Nurse")]
        [HttpGet("types")]
        public async Task<ActionResult<List<TestTypeLite>>> GetTypes(CancellationToken ct = default)
        {
            var types = await _service.GetTestTypesAsync(ct);
            return Ok(types);
        }
    }
}
