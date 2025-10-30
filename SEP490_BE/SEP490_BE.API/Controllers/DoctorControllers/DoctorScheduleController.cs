using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.BLL.IServices.IDoctorServices;

namespace SEP490_BE.API.Controllers.DoctorControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorScheduleController : ControllerBase
    {
        private readonly IDoctorScheduleService _service;

        public DoctorScheduleController(IDoctorScheduleService service)
        {
            _service = service;
        }

        [HttpGet("doctor-active-schedule-range/{doctorId}")]
        public async Task<IActionResult> GetDoctorActiveScheduleInRange(
               int doctorId,
               [FromQuery] DateOnly? startDate,
               [FromQuery] DateOnly? endDate)
        {
            try
            {
                var data = await _service.GetDoctorActiveScheduleInRangeAsync(
                    doctorId,
                    startDate ?? default,
                    endDate ?? default
                );

                if (data == null || data.Count == 0)
                {
                    return Ok(new
                    {
                        message = "Không có lịch làm việc cho bác sĩ trong khoảng thời gian này."
                    });
                }

                return Ok(data);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}