using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.TestResults;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestResultsController : ControllerBase
    {
        private readonly ITestResultService _service;
        public TestResultsController(ITestResultService service) => _service = service;

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
                visitDate = d;

            var q = new TestWorklistQueryDto
            {
                VisitDate = visitDate,   // ⬅ GIỮ NULL nếu không nhập ngày
                PatientName = patientName,
                RequiredState = state,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _service.GetWorklistAsync(q, ct);
            return Ok(result);
        }



        [HttpGet("record/{recordId:int}")]
        public async Task<ActionResult<List<ReadTestResultDto>>> GetByRecordId([FromRoute] int recordId, CancellationToken ct)
        {
            var items = await _service.GetByRecordIdAsync(recordId, ct);
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReadTestResultDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var item = await _service.GetByIdAsync(id, ct);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<ReadTestResultDto>> Create([FromBody] CreateTestResultDto dto, CancellationToken ct)
        {
            var created = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.TestResultId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ReadTestResultDto>> Update([FromRoute] int id, [FromBody] UpdateTestResultDto dto, CancellationToken ct)
        {
            var updated = await _service.UpdateAsync(id, dto, ct);
            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        [HttpGet("types")]
        public async Task<ActionResult<List<object>>> GetTypes(CancellationToken ct)
        {
            var types = await _service.GetTestTypesAsync(ct);
            return Ok(types.Select(t => new { t.TestTypeId, t.TestName }));
        }
    }
}
