using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices.IDoctorServices;
using SEP490_BE.BLL.IServices.IManagerService;
using SEP490_BE.BLL.IServices.IManagerServices;
using SEP490_BE.BLL.Services;
using SEP490_BE.BLL.Services.ManagerServices;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.ManagerDTO.ManagerSchedule;
using SEP490_BE.DAL.DTOs.ManagerDTO.Notification;
using SEP490_BE.DAL.DTOs.MedicineDTO;

namespace SEP490_BE.API.Controllers.ManagerControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Clinic Manager")]
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
        [AllowAnonymous]
        public async Task<IActionResult> GetAllShifts()
        {
            try
            {
                var data = await _service.GetAllShiftsAsync();
            return Ok(data);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Có lỗi xảy ra khi lấy danh sách ca làm việc." });
            }
        }
        // All bsi
        [HttpGet("doctors")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllDoctors([FromQuery] string? keyword)
        {
            try
            {

                var allDoctors = await _service.GetAllDoctorsAsync();
                return Ok(allDoctors);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Có lỗi xảy ra khi lấy danh sách bác sĩ." });
            }
        }
        [HttpGet("doctors2")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllDoctors2([FromQuery] string? keyword)
        {

            var allDoctors = await _service.GetAllDoctors2Async();
            return Ok(allDoctors);

        }

        // Tìm bác sĩ theo tên
        [HttpGet("doctors/search")]
        [AllowAnonymous]
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
            if (doctorId <= 0 || shiftId <= 0)
            {
                return BadRequest(new { message = "doctorId và shiftId phải lớn hơn 0." });
            }

            if (from > to)
            {
                return BadRequest(new { message = "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc." });
            }

            try
            {
                bool conflict = await _service.CheckDoctorConflictAsync(doctorId, shiftId, from, to);
            return Ok(new
            {
                isAvailable = !conflict,
                message = conflict ? "Bác sĩ đã có lịch trùng." : "Bác sĩ rảnh trong thời gian này."
            });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Có lỗi xảy ra khi kiểm tra lịch làm việc của bác sĩ." });
            }
        }

        //  Tạo lịch làm việc + gui thong bao tu dong
        [HttpPost("create-schedule")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleRequestDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Dữ liệu lịch làm việc là bắt buộc." });
            }

            if (dto.EffectiveFrom > dto.EffectiveTo)
            {
                return BadRequest(new { message = "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc." });
            }

            try
            {

                var created = await _service.CreateScheduleAsync(dto);
            var listDoctorIds = dto.Shifts
       .SelectMany(s => s.DoctorIds ?? new List<int>()) 
       .Distinct()
       .ToList();
                if (listDoctorIds.Any())
                {
                    var listReceive = await _doctorService.GetUserIdsByDoctorIdsAsync(listDoctorIds);

                    CreateNotificationDTO dtoNotify = new CreateNotificationDTO
                    {
                        Title = "Lịch làm việc mới",
                        Content = $"Lịch làm việc của bạn từ {dto.EffectiveFrom} đến {dto.EffectiveTo} đã được tạo. Vui lòng kiểm tra lịch làm việc của bạn.",
                        ReceiverIds = listReceive,
                        Type = "Schedule",
                        CreatedBy = null,
                    };
                    await _notificationService.SendNotificationAsync(dtoNotify);
                }
            return Ok(new { message = $"Tạo lịch làm việc thành công." });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Có lỗi xảy ra khi tạo lịch làm việc." });
            }
        }

  
        [HttpGet("get-all-doctor-schedule")]
        public async Task<IActionResult> GetAll([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            var result = await _doctorService.GetAllDoctorSchedulesByRangeAsync(startDate, endDate);
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

  

        //   Danh sach lịch  groupby EffectiveFrom, EffectiveTo, Shift
        [HttpGet("listGroupSchedule")]
        public async Task<IActionResult> GetGroupedWorkScheduleList(
      [FromQuery] int pageNumber = 1,
      [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest(new { message = "pageNumber và pageSize phải lớn hơn 0." });
            }

            try
            {
                var data = await _service.GetGroupedWorkScheduleListAsync(pageNumber, pageSize);
            return Ok(data);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Có lỗi xảy ra khi lấy danh sách lịch làm việc nhóm." });
            }
        }

        [HttpPut("update-doctor-shifts-range")]
        public async Task<IActionResult> UpdateDoctorShiftsInRange([FromBody] UpdateDoctorShiftRangeRequest request)
        {
            try
            {
                await _service.UpdateDoctorShiftsInRangeAsync(request);

                var addIds = request.AddDoctorIds ?? new List<int>();
                var removeIds = request.RemoveDoctorIds ?? new List<int>();

                if (addIds.Any())
                {
                    var receiversAdd = await _doctorService.GetUserIdsByDoctorIdsAsync(addIds);

                    if (receiversAdd.Any())
                    {
                        var notifyAdd = new CreateNotificationDTO
                        {
                            Title = "Lịch làm việc mới",
                            Content = $"Lịch làm việc của bạn từ {request.FromDate:dd/MM/yyyy} đến {request.ToDate:dd/MM/yyyy} đã được tạo. Vui lòng kiểm tra.",
                            ReceiverIds = receiversAdd,
                            Type = "schedule",
                            CreatedBy = null
                        };

                        await _notificationService.SendNotificationAsync(notifyAdd);
                    }
                }

                if (removeIds.Any())
                {
                    var receiversRemove = await _doctorService.GetUserIdsByDoctorIdsAsync(removeIds);

                    if (receiversRemove.Any())
                    {
                        var notifyRemove = new CreateNotificationDTO
                        {
                            Title = "Lịch làm việc thay đổi",
                            Content = $"Lịch làm việc của bạn từ {request.FromDate:dd/MM/yyyy} đến {request.ToDate:dd/MM/yyyy} đã thay đổi. Vui lòng kiểm tra.",
                            ReceiverIds = receiversRemove,
                            Type = "schedule",
                            CreatedBy = null
                        };

                        await _notificationService.SendNotificationAsync(notifyRemove);
                    }
                }

                return Ok(new { message = "Cập nhật lịch làm việc thành công." });
            }
            catch (Exception ex)
            {
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

        [HttpGet("doctors-without-schedule")]
        public async Task<IActionResult> GetDoctorsWithoutSchedule(
    [FromQuery] DateOnly startDate,
    [FromQuery] DateOnly endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest(new { message = "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc." });
            }

            try
            {
                var result = await _service.GetDoctorsWithoutScheduleAsync(startDate, endDate);
            return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Có lỗi xảy ra khi lấy danh sách bác sĩ chưa có lịch làm việc." });
            }
        }

    }
}
