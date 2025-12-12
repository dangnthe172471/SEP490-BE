using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.PrescriptionDoctorDTO;
using System.Security.Claims;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionsDoctorController : ControllerBase
    {
        private readonly IPrescriptionDoctorService _service;

        public PrescriptionsDoctorController(IPrescriptionDoctorService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost]
        public async Task<ActionResult> Create(
    [FromBody] CreatePrescriptionRequest req,
    CancellationToken ct = default)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _service.CreateAsync(userId, req, ct);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.PrescriptionId },
                    new
                    {
                        message = "Tạo đơn thuốc thành công",
                        data = result
                    }
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [Authorize(Roles = "Doctor,Patient")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PrescriptionSummaryDto>> GetById(
            int id,
            CancellationToken ct = default)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _service.GetByIdAsync(userId, id, ct);
                return result is null ? NotFound() : Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("records")]
        public async Task<ActionResult<PagedResult<RecordListItemDto>>> GetRecordsForDoctor(
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            // Clamp pageNumber & pageSize
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

            try
            {
                var result = await _service.GetRecordsForDoctorAsync(
                    userId, from, to, search, pageNumber, pageSize, ct);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
