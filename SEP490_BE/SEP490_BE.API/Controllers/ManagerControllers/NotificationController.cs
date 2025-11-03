using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices.IManagerService;
using SEP490_BE.DAL.DTOs.ManagerDTO.Notification;

namespace SEP490_BE.API.Controllers.NotificationControllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Clinic Manager")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;


        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
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
            await _notificationService.SendNotificationAsync(dto);
            return Ok(new { Message = "Notification sent successfully!" });
        }
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(int userId, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _notificationService.GetNotificationsByUserAsync(userId, pageNumber, pageSize);
            return Ok(result);
        }

        [HttpPut("read/{userId}/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(int userId, int notificationId)
        {
            var result = await _notificationService.MarkAsReadAsync(userId, notificationId);
            if (!result)
                return NotFound(new { message = "Không tìm thấy thông báo hoặc user tương ứng." });

            return Ok(new { message = "Đã đánh dấu thông báo là đã đọc." });
        }


        [HttpGet("unread-count/{userId}")]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            var count = await _notificationService.CountUnreadAsync(userId);
            return Ok(count);
        }
        [HttpPut("read-all/{userId}")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            await _notificationService.MarkAllAsReadAsync(userId);
            return NoContent();
        }
        [HttpGet("list-notification")]
        public async Task<IActionResult> GetAllNotifications( int pageNumber = 1, int pageSize = 10)
        {
            var result = await _notificationService.GetListNotificationsAsync( pageNumber, pageSize);
            return Ok(result);
        }
    }
}
