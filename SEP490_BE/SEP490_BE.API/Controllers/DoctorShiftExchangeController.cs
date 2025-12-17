using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorShiftExchangeController : ControllerBase
    {
        private readonly IDoctorShiftExchangeService _service;

        public DoctorShiftExchangeController(IDoctorShiftExchangeService service)
        {
            _service = service;
        }

        [HttpPost("create-request")]
        public async Task<IActionResult> CreateShiftSwapRequest([FromBody] CreateShiftSwapRequestDTO request)
        {
            try
            {
                var result = await _service.CreateShiftSwapRequestAsync(request);
                return Ok(new { success = true, data = result, message = "Yêu cầu đổi ca đã được tạo thành công" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("doctor/{doctorId}/requests")]
        public async Task<IActionResult> GetRequestsByDoctorId(int doctorId)
        {
            try
            {
                var requests = await _service.GetRequestsByDoctorIdAsync(doctorId);
                return Ok(new { success = true, data = requests });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("{exchangeId}")]
        public async Task<IActionResult> GetRequestById(int exchangeId)
        {
            try
            {
                var request = await _service.GetRequestByIdAsync(exchangeId);
                if (request == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy yêu cầu đổi ca" });
                }
                return Ok(new { success = true, data = request });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost("review-request")]
        public async Task<IActionResult> ReviewShiftSwapRequest([FromBody] ReviewShiftSwapRequestDTO review)
        {
            try
            {
                var result = await _service.ReviewShiftSwapRequestAsync(review);
                if (result)
                {
                    var message = review.Status == "Approved" ? "Yêu cầu đổi ca đã được chấp nhận" : "Yêu cầu đổi ca đã bị từ chối";
                    return Ok(new { success = true, message = message });
                }
                return BadRequest(new { success = false, message = "Không thể cập nhật trạng thái yêu cầu" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("doctor/{doctorId}/shifts")]
        public async Task<IActionResult> GetDoctorShifts(int doctorId, [FromQuery] DateOnly from, [FromQuery] DateOnly to)
        {
            try
            {
                var shifts = await _service.GetDoctorShiftsAsync(doctorId, from, to);
                return Ok(new { success = true, data = shifts });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("doctors/specialty/{specialty}")]
        public async Task<IActionResult> GetDoctorsBySpecialty(string specialty)
        {
            try
            {
                var doctors = await _service.GetDoctorsBySpecialtyAsync(specialty);
                return Ok(new { success = true, data = doctors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost("validate-request")]
        public async Task<IActionResult> ValidateShiftSwapRequest([FromBody] CreateShiftSwapRequestDTO request)
        {
            try
            {
                var isValid = await _service.ValidateShiftSwapRequestAsync(request);
                if (!isValid)
                {
                    return BadRequest(new { success = false, message = "Yêu cầu đổi ca không hợp lệ" });
                }
                
                return Ok(new { success = true, message = "Yêu cầu hợp lệ" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("doctor-id/user/{userId}")]
        public async Task<IActionResult> GetDoctorIdByUserId(int userId)
        {
            try
            {
                var doctorId = await _service.GetDoctorIdByUserIdAsync(userId);
                if (!doctorId.HasValue)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy bác sĩ với userId này" });
                }
                return Ok(new { success = true, data = doctorId.Value });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("doctor/user/{userId}")]
        public async Task<IActionResult> GetDoctorByUserId(int userId)
        {
            try
            {
                var doctor = await _service.GetDoctorByUserIdAsync(userId);
                if (doctor == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy bác sĩ với userId này" });
                }
                return Ok(new { success = true, data = doctor });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}
