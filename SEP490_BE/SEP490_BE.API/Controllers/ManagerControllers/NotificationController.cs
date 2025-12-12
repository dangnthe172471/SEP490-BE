using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.BLL.IServices.IManagerService;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.DTOs.ManagerDTO.Notification;

namespace SEP490_BE.API.Controllers.NotificationControllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Clinic Manager")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        private readonly IAdministratorService _administratorService;

     
        public NotificationController(INotificationService notificationService, IAdministratorService administratorService)
        {
            _notificationService = notificationService;
            _administratorService = administratorService;
        }

        [HttpGet("all-user")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll(CancellationToken cancellationToken)
        {
            var users = await _administratorService.GetAllAsync(cancellationToken);
            return Ok(users);
        }


        [HttpPost("send-reminder")]
        public async Task<IActionResult> SendReminder()
        {
            await _notificationService.SendAppointmentReminderAsync();
            return Ok(new
            {
                Success = true,
                Message = "Đã gửi email nhắc lịch thành công cho các bệnh nhân có lịch khám ngày mai!"
            });
        }
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] CreateNotificationDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Notification data is required." });
            }

            if (string.IsNullOrWhiteSpace(dto.Title) ||
                string.IsNullOrWhiteSpace(dto.Content))
            {
                return BadRequest(new { message = "Title and content are required." });
            }

            if (dto.CreatedBy <= 0)
            {
                return BadRequest(new { message = "CreatedBy must be greater than 0." });
            }

            try
            {
                await _notificationService.SendNotificationAsync(dto);
            return Ok(new { Message = "Notification sent successfully!" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while sending notification." });
            }
        }
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(int userId, int pageNumber = 1, int pageSize = 10)
        {
            if (userId <= 0 || pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest(new { message = "userId, pageNumber và pageSize phải lớn hơn 0." });
            }

            try
            {
                var result = await _notificationService.GetNotificationsByUserAsync(userId, pageNumber, pageSize);
            return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Không tìm thấy user tương ứng." });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Có lỗi xảy ra khi lấy danh sách thông báo của user." });
            }
        }

        [HttpPut("read/{userId}/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(int userId, int notificationId)
        {
            if (userId <= 0 || notificationId <= 0)
            {
                return BadRequest(new { message = "userId và notificationId phải > 0." });
            }
            try
            {
                var result = await _notificationService.MarkAsReadAsync(userId, notificationId);
            if (!result)
                return NotFound(new { message = "Không tìm thấy thông báo hoặc user tương ứng." });

            return Ok(new { message = "Đã đánh dấu thông báo là đã đọc." });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Có lỗi xảy ra khi cập nhật trạng thái thông báo." });
            }
        }


        [HttpGet("unread-count/{userId}")]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new { message = "userId phải lớn hơn 0." });
            }
            try
            {
                var count = await _notificationService.CountUnreadAsync(userId);
            return Ok(count);
            
            }
            catch (KeyNotFoundException)
            {
               
                return NotFound(new { message = "Không tìm thấy user tương ứng." });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Có lỗi xảy ra khi lấy số lượng thông báo chưa đọc." });
            }
        }
        [HttpPut("read-all/{userId}")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new { message = "userId must be greater than 0." });
            }

            try
            {
                await _notificationService.MarkAllAsReadAsync(userId);
            return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Không tìm thấy user tương ứng." });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Có lỗi xảy ra khi cập nhật trạng thái thông báo." });
            }
        }
        [HttpGet("list-notification")]
        public async Task<IActionResult> GetAllNotifications( int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest(new { message = "pageNumber và pageSize phải lớn hơn 0." });
            }

            try
            {
                var result = await _notificationService.GetListNotificationsAsync( pageNumber, pageSize);
            return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Có lỗi xảy ra khi lấy danh sách thông báo." });
            }
        }
    }
}
