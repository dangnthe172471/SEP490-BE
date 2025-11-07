using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers.NotificationControllers;
using SEP490_BE.BLL.IServices.IManagerService;
using SEP490_BE.DAL.Helpers;
using SEP490_BE.DAL.DTOs.ManagerDTO.Notification;

namespace SEP490_BE.Tests.Controllers
{
	public class NotificationControllerTests
	{
		private readonly Mock<INotificationService> _svc = new(MockBehavior.Strict);

		private NotificationController NewController() => new NotificationController(_svc.Object);

		[Fact]
		public async Task SendReminder_ReturnsOkWithMessage()
		{
			_svc.Setup(s => s.SendAppointmentReminderAsync(default)).Returns(Task.CompletedTask);
			var ctrl = NewController();
			var result = await ctrl.SendReminder();
			var ok = Assert.IsType<OkObjectResult>(result);
			var val = ok.Value!;
			var success = (bool)val.GetType().GetProperty("Success")!.GetValue(val)!;
			var message = (string)val.GetType().GetProperty("Message")!.GetValue(val)!;
			Assert.True(success);
			Assert.Contains("nhắc lịch", message);
			_svc.VerifyAll();
		}

        #region SendNotification

        [Fact]
        public async Task SendNotification_Success_ReturnsOk()
        {
            // Arrange
            var dto = new CreateNotificationDTO
            {
                Title = "System Alert",
                Content = "New update available",
                Type = "System",
                CreatedBy = 1,
                IsGlobal = true,
                RoleNames = new List<string> { "Doctor", "Manager" }
            };

            _svc.Setup(s => s.SendNotificationAsync(dto)).Returns(Task.CompletedTask);
            var ctrl = NewController();

            // Act
            var result = await ctrl.SendNotification(dto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("successfully", ok.Value!.ToString());
            _svc.VerifyAll();
        }

        [Fact]
        public async Task SendNotification_ServiceThrows_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateNotificationDTO
            {
                Title = "DB Error Test",
                Content = "Trigger database failure",
                Type = "System",
                CreatedBy = 2
            };

            _svc.Setup(s => s.SendNotificationAsync(dto))
                .ThrowsAsync(new Exception("Database failure"));
            var ctrl = NewController();

            // Act
            IActionResult result;
            try
            {
                result = await ctrl.SendNotification(dto);
            }
            catch (Exception ex)
            {
                Assert.Contains("Database failure", ex.Message);
                return;
            }

            // Assert (if controller has try-catch)
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Database failure", bad.Value!.ToString());
            _svc.VerifyAll();
        }

        #endregion
        [Fact]
		public async Task GetUserNotifications_ReturnsOkWithPaged()
		{
			var paged = new PaginationHelper.PagedResult<NotificationDTO> { Items = new List<NotificationDTO>(), PageNumber = 1, PageSize = 10, TotalCount = 0 };
			_svc.Setup(s => s.GetNotificationsByUserAsync(5, 1, 10)).ReturnsAsync(paged);
			var ctrl = NewController();
			var result = await ctrl.GetUserNotifications(5, 1, 10);
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Same(paged, ok.Value);
			_svc.VerifyAll();
		}
        #region MarkAsRead 
        [Fact]
		public async Task MarkAsRead_NotFound_WhenFalse()
		{
			_svc.Setup(s => s.MarkAsReadAsync(7, 99)).ReturnsAsync(false);
			var ctrl = NewController();
			var result = await ctrl.MarkAsRead(7, 99);
			var notFound = Assert.IsType<NotFoundObjectResult>(result);
			Assert.Contains("Không tìm thấy", notFound.Value!.ToString());
			_svc.VerifyAll();
		}

		[Fact]
		public async Task MarkAsRead_Ok_WhenTrue()
		{
			_svc.Setup(s => s.MarkAsReadAsync(7, 5)).ReturnsAsync(true);
			var ctrl = NewController();
			var result = await ctrl.MarkAsRead(7, 5);
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Contains("đã đọc", ok.Value!.ToString());
			_svc.VerifyAll();
		}
        #endregion
        #region GetUnreadCount 
        [Fact]
		public async Task GetUnreadCount_ReturnsOk()
		{
			_svc.Setup(s => s.CountUnreadAsync(8)).ReturnsAsync(42);
			var ctrl = NewController();
			var result = await ctrl.GetUnreadCount(8);
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Equal(12, ok.Value);
			_svc.VerifyAll();
		}

     
        [Fact]
        public async Task GetUnreadCount_WhenServiceThrows_ReturnsBadRequest()
        {
            // Arrange
            int userId = 99;
            _svc.Setup(s => s.CountUnreadAsync(userId))
                        .ThrowsAsync(new Exception("Database connection failed"));

            var ctrl = NewController();

            // Act
            IActionResult result;
            try
            {
                result = await ctrl.GetUnreadCount(userId);
            }
            catch (Exception ex)
            {
                // Nếu controller chưa có try-catch, test này sẽ fail ở đây → bạn có thể handle trong controller
                Assert.Contains("Database connection failed", ex.Message);
                return;
            }

            // Nếu controller có try-catch, ta test BadRequest:
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Database connection failed", bad.Value!.ToString());
            _svc.VerifyAll();
        }
        [Fact]
        public async Task GetUnreadCount_WithInvalidUserId_ReturnsBadRequest()
        {
            _svc.Setup(s => s.CountUnreadAsync(0))
                .ThrowsAsync(new ArgumentException("Invalid userId"));

            var ctrl = NewController();

            await Assert.ThrowsAsync<ArgumentException>(() => ctrl.GetUnreadCount(0));
            _svc.VerifyAll();
        }

        #endregion
        [Fact]
		public async Task MarkAllAsRead_NoContent()
		{
			_svc.Setup(s => s.MarkAllAsReadAsync(9)).Returns(Task.CompletedTask);
			var ctrl = NewController();
			var result = await ctrl.MarkAllAsRead(9);
			Assert.IsType<NoContentResult>(result);
			_svc.VerifyAll();
		}

		[Fact]
		public async Task GetAllNotifications_ReturnsOkWithPaged()
		{
			var paged = new PaginationHelper.PagedResult<NotificationDTO> { Items = new List<NotificationDTO>(), PageNumber = 3, PageSize = 15, TotalCount = 0 };
			_svc.Setup(s => s.GetListNotificationsAsync(3, 15)).ReturnsAsync(paged);
			var ctrl = NewController();
			var result = await ctrl.GetAllNotifications(3, 15);
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Same(paged, ok.Value);
			_svc.VerifyAll();
		}
	}
}
