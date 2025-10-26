using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace SEP490_BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorShiftExchangeController : ControllerBase
    {
        private readonly IDoctorShiftExchangeService _service;
        private readonly IDoctorShiftExchangeRepository _repository;

        public DoctorShiftExchangeController(IDoctorShiftExchangeService service, IDoctorShiftExchangeRepository repository)
        {
            _service = service;
            _repository = repository;
        }

        [HttpPost("create-request")]
        public async Task<IActionResult> CreateShiftSwapRequest([FromBody] CreateShiftSwapRequestDTO request)
        {
            try
            {
                // Debug validation
                var debugInfo = new List<string>();
                
                // Check basic validation
                if (request.Doctor1Id <= 0 || request.Doctor2Id <= 0)
                {
                    debugInfo.Add("Invalid doctor IDs");
                }
                
                if (request.Doctor1Id == request.Doctor2Id)
                {
                    debugInfo.Add("Cannot swap with yourself");
                }
                
                var result = await _service.CreateShiftSwapRequestAsync(request);
                return Ok(new { success = true, data = result, message = "Yêu cầu đổi ca đã được tạo thành công" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message, debug = "Validation failed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("pending-requests")]
        public async Task<IActionResult> GetPendingRequests()
        {
            try
            {
                var allRequests = await _service.GetAllRequestsAsync();
                var pendingRequests = allRequests.Where(r => r.Status == "Pending").ToList();
                return Ok(new { success = true, data = pendingRequests });
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
                // Debug: Log the specialty being searched
                Console.WriteLine($"Searching for doctors with specialty: '{specialty}'");
                
                var doctors = await _service.GetDoctorsBySpecialtyAsync(specialty);
                Console.WriteLine($"Found {doctors.Count} doctors");
                
                return Ok(new { success = true, data = doctors, debug = new { specialty, count = doctors.Count } });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting doctors by specialty: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("doctors/all")]
        public async Task<IActionResult> GetAllDoctors()
        {
            try
            {
                var doctors = await _service.GetAllDoctorsAsync();
                return Ok(new { success = true, data = doctors, count = doctors.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("specialties")]
        public async Task<IActionResult> GetSpecialties()
        {
            try
            {
                var specialties = await _service.GetSpecialtiesAsync();
                return Ok(new { success = true, data = specialties });
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
                var debugInfo = new List<string>();
                
                // Check basic validation
                if (request.Doctor1Id <= 0 || request.Doctor2Id <= 0)
                {
                    debugInfo.Add("Invalid doctor IDs");
                }
                
                if (request.Doctor1Id == request.Doctor2Id)
                {
                    debugInfo.Add("Cannot swap with yourself");
                }
                
                // Check if doctors exist and have same specialty
                var doctor1 = await _service.GetDoctorByIdAsync(request.Doctor1Id);
                var doctor2 = await _service.GetDoctorByIdAsync(request.Doctor2Id);
                
                if (doctor1 == null) debugInfo.Add("Doctor1 not found");
                if (doctor2 == null) debugInfo.Add("Doctor2 not found");
                
                if (doctor1 != null && doctor2 != null)
                {
                    if (doctor1.Specialty != doctor2.Specialty)
                    {
                        debugInfo.Add($"Different specialties: {doctor1.Specialty} vs {doctor2.Specialty}");
                    }
                }
                
                // Check if shifts exist using the repository method
                var doctor1HasShift = await _repository.HasExistingShiftAsync(request.Doctor1Id, request.Doctor1ShiftRefId, request.ExchangeDate);
                var doctor2HasShift = await _repository.HasExistingShiftAsync(request.Doctor2Id, request.Doctor2ShiftRefId, request.ExchangeDate);
                
                if (!doctor1HasShift) debugInfo.Add("Doctor1 doesn't have the specified shift");
                if (!doctor2HasShift) debugInfo.Add("Doctor2 doesn't have the specified shift");
                
                // Check pending requests
                var hasPending = await _service.ValidateShiftSwapRequestAsync(request);
                
                if (debugInfo.Any())
                {
                    return BadRequest(new { success = false, message = "Validation failed", debug = debugInfo });
                }
                
                return Ok(new { success = true, message = "Request is valid" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
