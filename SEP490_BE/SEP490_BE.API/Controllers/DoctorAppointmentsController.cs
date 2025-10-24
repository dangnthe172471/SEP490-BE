using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using System.Security.Claims;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorAppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _service;

        public DoctorAppointmentsController(IAppointmentService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("appointments")]
        public async Task<IActionResult> GetMyAppointments(CancellationToken ct = default)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirstValue("sub");
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { message = "Token không hợp lệ (thiếu UserId)." });

            var result = await _service.GetDoctorAppointmentsAsync(userId, ct);
            return Ok(result);
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("appointments/{appointmentId:int}")]
        public async Task<IActionResult> GetMyAppointmentDetail(
            int appointmentId,
            CancellationToken ct = default)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirstValue("sub");
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { message = "Token không hợp lệ (thiếu UserId)." });

            var dto = await _service.GetDoctorAppointmentDetailAsync(userId, appointmentId, ct);
            if (dto is null)
                return NotFound(new { message = "Không tìm thấy lịch hẹn hoặc không thuộc bác sĩ hiện tại." });

            return Ok(dto);
        }
    }
}
