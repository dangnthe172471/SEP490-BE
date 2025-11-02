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
    }
}
