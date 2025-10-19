using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestTypesController : ControllerBase
    {
        private readonly ITestTypeService _testTypeService;

        public TestTypesController(ITestTypeService testTypeService)
        {
            _testTypeService = testTypeService;
        }


        [HttpGet]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<IEnumerable<TestTypeDto>>> GetAll(CancellationToken cancellationToken)
        {
            var testTypes = await _testTypeService.GetAllAsync(cancellationToken);
            return Ok(testTypes);
        }


        [HttpGet("paged")]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<PagedResponse<TestTypeDto>>> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _testTypeService.GetPagedAsync(pageNumber, pageSize, searchTerm, cancellationToken);
            return Ok(result);
        }


        [HttpGet("{id}")]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<TestTypeDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var testType = await _testTypeService.GetByIdAsync(id, cancellationToken);
            if (testType == null)
            {
                return NotFound();
            }

            return Ok(testType);
        }


        [HttpPost]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateTestTypeRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var id = await _testTypeService.CreateAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<TestTypeDto>> Update(
            int id,
            [FromBody] UpdateTestTypeRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _testTypeService.UpdateAsync(id, request, cancellationToken);
                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var success = await _testTypeService.DeleteAsync(id, cancellationToken);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
