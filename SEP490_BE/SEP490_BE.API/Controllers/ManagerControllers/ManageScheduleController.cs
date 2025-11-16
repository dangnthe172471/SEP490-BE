using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices.IDoctorServices;
using SEP490_BE.BLL.IServices.IManagerService;
using SEP490_BE.BLL.IServices.IManagerServices;
using SEP490_BE.BLL.Services;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.ManagerDTO.ManagerSchedule;
using SEP490_BE.DAL.DTOs.ManagerDTO.Notification;
using SEP490_BE.DAL.DTOs.MedicineDTO;

namespace SEP490_BE.API.Controllers.ManagerControllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Clinic Manager")]
    public class ManageScheduleController : ControllerBase
    {
        private readonly IScheduleService _service;
        private readonly INotificationService _notificationService;
        private readonly IDoctorScheduleService _doctorService;
        public ManageScheduleController(IScheduleService service, INotificationService notificationService, IDoctorScheduleService doctorService) { 
            _service = service;
            _notificationService = notificationService;
            _doctorService = doctorService;
        }

        // Lấy danh sách ca làm việc
        [HttpGet("shifts")]
        public async Task<IActionResult> GetAllShifts()
        {
            var data = await _service.GetAllShiftsAsync();
            return Ok(data);
        }
        // All bsi
        [HttpGet("doctors")]
        public async Task<IActionResult> GetAllDoctors([FromQuery] string? keyword)
        {
          
                var allDoctors = await _service.GetAllDoctorsAsync();
                return Ok(allDoctors);
          
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

        //  Tạo lịch làm việc + gui thong bao tu dong
        [HttpPost("create-schedule")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleRequestDTO dto)
        {
         
      
            var created = await _service.CreateScheduleAsync(dto);
            var listDoctorIds = dto.Shifts
       .SelectMany(s => s.DoctorIds ?? new List<int>()) 
       .Distinct()
       .ToList();
           var listReceive =  await _doctorService.GetUserIdsByDoctorIdsAsync(listDoctorIds);

            CreateNotificationDTO dtoNotify = new CreateNotificationDTO
            {
               Title = "Lịch làm việc mới",
                Content = $"Lịch làm việc của bạn từ {dto.EffectiveFrom} đến {dto.EffectiveTo} đã được tạo. Vui lòng kiểm tra lịch làm việc của bạn.",
                ReceiverIds = listReceive,
                Type = "Schedule",
                CreatedBy = null,
            };
            await _notificationService.SendNotificationAsync(dtoNotify);
            return Ok(new { message = $"Tạo thành công {created} lịch làm việc." });
        }

        // Xem lịch làm việc --
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
        [HttpGet("get-all-doctor-schedule")]
        public async Task<IActionResult> GetAll([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            var result = await _doctorService.GetAllDoctorSchedulesByRangeAsync(startDate, endDate);
            return Ok(result);
        }
        //--
        [HttpGet("allSchedules")]
        public async Task<ActionResult<PagedResult<WorkScheduleDto>>> GetAllSchedule(
         [FromQuery] int pageNumber = 1,
         [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllSchedulesAsync(pageNumber, pageSize);
            return Ok(result);
        }

        // xem lịch  theo range
        [HttpGet("getScheduleByRange")]
        public async Task<ActionResult<List<DailyWorkScheduleViewDto>>> GetByRange(
     [FromQuery] DateOnly start,
     [FromQuery] DateOnly end)
        {
            var result = await _service.GetWorkScheduleByDateRangeAsync(start, end);
            return Ok(result);
        }

        // xem lịch  theo ngày
        [HttpGet("getScheduleByDate")]
        public async Task<ActionResult<PagedResult<DailyWorkScheduleDto>>> GetByDate(
           [FromQuery] DateOnly? date,
           [FromQuery] int pageNumber = 1,
           [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetWorkSchedulesByDateAsync(date, pageNumber, pageSize);
            return Ok(result);
        }

        //Tông quan lịch theo tháng
        [HttpGet("monthly-summary")]
        public async Task<ActionResult<List<DailySummaryDto>>> GetMonthlyWorkSummary([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var result = await _service.GetMonthlyWorkSummaryAsync(year, month);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //  Thêm / xóa bác sĩ  của lịch theo ngày --
        [HttpPut("updateScheduleByDate")]
        public async Task<IActionResult> UpdateByDate([FromBody] UpdateWorkScheduleByDateRequest request)
        {
            try
            {
                await _service.UpdateWorkScheduleByDateAsync(request);
                return Ok(new { message = "Cập nhật lịch theo ngày thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Cập nhật lịch theo ID lịch (DoctorShiftId) --
        [HttpPut("updateScheduleByScheduleId")]
        public async Task<IActionResult> UpdateById([FromBody] UpdateWorkScheduleByIdRequest request)
        {
            await _service.UpdateWorkScheduleByIdAsync(request);
            return Ok(new { message = "Cập nhật lịch thành công!" });
        }

        //   Danh sach lịch  groupby EffectiveFrom, EffectiveTo, Shift
        [HttpGet("listGroupSchedule")]
        public async Task<IActionResult> GetGroupedWorkScheduleList(
      [FromQuery] int pageNumber = 1,
      [FromQuery] int pageSize = 10)
        {
            var data = await _service.GetGroupedWorkScheduleListAsync(pageNumber, pageSize);
            return Ok(data);
        }

        [HttpPut("update-doctor-shifts-range")]
        public async Task<IActionResult> UpdateDoctorShiftsInRange([FromBody] UpdateDoctorShiftRangeRequest request)
        {
            try
            {
                await _service.UpdateDoctorShiftsInRangeAsync(request);
                return Ok(new { message = " Cập nhật lịch làm việc thành công." });
            }
            catch (Exception ex)
            {
                // Ghi log (nếu có ILogger)
                Console.WriteLine($"[ERROR] UpdateDoctorShiftsInRangeAsync: {ex.Message}");

                // Trả thông tin lỗi cho FE
                return BadRequest(new
                {
                    error = ex.Message,
                    detail = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("check-limit")]
        public async Task<bool> CheckDoctorShiftLimit([FromQuery] int doctorId, [FromQuery] DateOnly date)
        {
            if (doctorId <= 0) return false;
            return await _service.CheckDoctorShiftLimitAsync(doctorId, date);
        }

        [HttpGet("check-limit-range")]
        public async Task<IActionResult> CheckDoctorShiftLimitRange([FromQuery] int doctorId, [FromQuery] DateOnly from, [FromQuery] DateOnly to)
        {
            try
            {
                var result = await _service.CheckDoctorShiftLimitRangeAsync(doctorId, from, to);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
