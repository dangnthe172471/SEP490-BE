using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;

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
    }
}
