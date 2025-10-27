using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.PrescriptionDoctorDTO;
using System.Security.Claims;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionsDoctorController : ControllerBase
    {
        private readonly IPrescriptionDoctorService _service;
        public PrescriptionsDoctorController(IPrescriptionDoctorService service) => _service = service;

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<PrescriptionSummaryDto>> Create([FromBody] CreatePrescriptionRequest req, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var result = await _service.CreateAsync(userId, req, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.PrescriptionId }, result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Doctor,Patient,Receptionist,Pharmacy Provider")]
        public async Task<ActionResult<PrescriptionSummaryDto>> GetById(int id, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            _ = int.TryParse(userIdStr, out var userId);

            var result = await _service.GetByIdAsync(userId, id, ct);
            return result is null ? NotFound() : Ok(result);
        }
    }
}
