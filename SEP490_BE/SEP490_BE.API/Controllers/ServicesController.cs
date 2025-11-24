using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _serviceService;

        public ServicesController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        [HttpGet]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<IEnumerable<ServiceDto>>> GetAll(CancellationToken cancellationToken)
        {
            var services = await _serviceService.GetAllAsync(cancellationToken);
            return Ok(services);
        }

        [HttpGet("paged")]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<PagedResponse<ServiceDto>>> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _serviceService.GetPagedAsync(pageNumber, pageSize, searchTerm, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<ServiceDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var service = await _serviceService.GetByIdAsync(id, cancellationToken);
            if (service == null)
            {
                return NotFound(new { message = "Service not found." });
            }

            return Ok(service);
        }

        [HttpPost]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateServiceRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var id = await _serviceService.CreateAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<ServiceDto>> Update(
            int id,
            [FromBody] UpdateServiceRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _serviceService.UpdateAsync(id, request, cancellationToken);
                if (result == null)
                {
                    return NotFound(new { message = "Service not found." });
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                var success = await _serviceService.DeleteAsync(id, cancellationToken);
                if (!success)
                {
                    return NotFound(new { message = "Service not found." });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

