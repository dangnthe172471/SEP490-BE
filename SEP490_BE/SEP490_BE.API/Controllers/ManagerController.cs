using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        private readonly IManagerService _service;
        public ManagerController(IManagerService service) => _service = service;

        // Lấy danh sách ca làm việc
        [HttpGet("shifts")]
        public async Task<IActionResult> GetAllShifts()
        {
            var data = await _service.GetAllShiftsAsync();
            return Ok(data);
        }

        // Tìm bác sĩ theo tên
        [HttpGet("doctors/search")]
        public async Task<IActionResult> SearchDoctors([FromQuery] string? keyword)
        {
            if(string.IsNullOrWhiteSpace(keyword))
            {
                var allDoctors = await _service.GetAllDoctorsAsync();
                return Ok(allDoctors);
            }
            var data = await _service.SearchDoctorsAsync(keyword);
            return Ok(data);
        }

        // Kiểm tra trùng lịch bác sĩ
        [HttpGet("check-conflict")]
        public async Task<IActionResult> CheckDoctorAvailability(int doctorId, int shiftId, DateOnly from, DateOnly to)
        {
            bool conflict = await _service.CheckDoctorConflictAsync(doctorId, shiftId, from, to);
            return Ok(new
            {
                isAvailable = !conflict,
                message = conflict ? "Bác sĩ đã có lịch trùng." : "Bác sĩ rảnh trong thời gian này."
            });
        }

        //  Tạo lịch làm việc
        [HttpPost("create-schedule")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleRequestDTO dto)
        {
            var created = await _service.CreateScheduleAsync(dto);
            return Ok(new { message = $"Tạo thành công {created} lịch làm việc." });
        }

        // Xem lịch làm việc
        [HttpGet("schedules")]
        public async Task<IActionResult> GetSchedules(DateOnly from, DateOnly to)
        {
            var list = await _service.GetSchedulesAsync(from, to);
            var result = list.Select(x => new
            {
                x.DoctorShiftId,
                Doctor = x.Doctor.User.FullName,
                Shift = x.Shift.ShiftType,
                Time = $"{x.Shift.StartTime:HH:mm} - {x.Shift.EndTime:HH:mm}",
                Date = x.EffectiveFrom.ToString("yyyy-MM-dd"),
                x.Status
            });
            return Ok(result);
        }
    }
}
