using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers.NotificationControllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.BLL.IServices.IManagerService;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.DTOs.ManagerDTO.Notification;
using SEP490_BE.DAL.Helpers;
using System.Linq;

namespace SEP490_BE.Tests.Controllers
{
	public class NotificationControllerTests
	{
		private readonly Mock<INotificationService> _svc = new(MockBehavior.Strict);
        private readonly Mock<IAdministratorService> _adminSvc = new(MockBehavior.Strict);

        private NotificationController NewController()
        => new NotificationController(_svc.Object, _adminSvc.Object);

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
        public async Task SendNotification_NullDto_ReturnsBadRequest()
        {
            var ctrl = NewController();
            var result = await ctrl.SendNotification(null!);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Notification data is required", bad.Value!.ToString());

            _svc.Verify(s => s.SendNotificationAsync(It.IsAny<CreateNotificationDTO>()), Times.Never);
        }

        [Fact]
        public async Task SendNotification_InvalidDto_ReturnsBadRequest()
        {
        
            var dto = new CreateNotificationDTO
            {
                Title = "", 
                Content = "Content here",
                Type = "System",
                CreatedBy = 1
            };

            var ctrl = NewController();
            var result = await ctrl.SendNotification(dto);

      
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Title and content are required", bad.Value!.ToString());

            _svc.Verify(s => s.SendNotificationAsync(It.IsAny<CreateNotificationDTO>()), Times.Never);
        }

        [Fact]
        public async Task SendNotification_InvalidCreatedBy_ReturnsBadRequest()
        {
            var dto = new CreateNotificationDTO
            {
                Title = "Test",
                Content = "Content",
                Type = "System",
                CreatedBy = 0
            };

            var ctrl = NewController();
            var result = await ctrl.SendNotification(dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("CreatedBy must be greater than 0", bad.Value!.ToString());

            _svc.Verify(s => s.SendNotificationAsync(It.IsAny<CreateNotificationDTO>()), Times.Never);
        }
        [Fact]
        public async Task SendNotification_ServiceThrows_Returns500()
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
            var result = await ctrl.SendNotification(dto);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            Assert.Contains("An error occurred while sending notification", obj.Value!.ToString());
            _svc.VerifyAll();
        }

        #endregion

        #region GetUserNotifications

        [Fact]
        public async Task GetUserNotifications_Success_ReturnsOkWithPagedResult()
        {
            // Arrange
            int userId = 7;
            int pageNumber = 1;
            int pageSize = 10;

            var items = new List<NotificationDTO>
    {
        new NotificationDTO
        {
            NotificationId = 1,
            Title = "N1",
            Content = "C1",
            Type = "System",
            CreatedDate = DateTime.UtcNow,
            IsRead = false
        },
        new NotificationDTO
        {
            NotificationId = 2,
            Title = "N2",
            Content = "C2",
            Type = "Reminder",
            CreatedDate = DateTime.UtcNow,
            IsRead = true
        }
    };

            var paged = new PaginationHelper.PagedResult<NotificationDTO>
            {
                Items = items,
                TotalCount = 2,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _svc.Setup(s => s.GetNotificationsByUserAsync(userId, pageNumber, pageSize))
                .ReturnsAsync(paged);

            var ctrl = NewController();

            // Act
            var result = await ctrl.GetUserNotifications(userId, pageNumber, pageSize);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<PaginationHelper.PagedResult<NotificationDTO>>(ok.Value);

            Assert.Equal(2, value.TotalCount);
            Assert.Equal(2, value.Items.Count);
            Assert.Equal("N1", value.Items[0].Title);

            _svc.VerifyAll();
        }
        [Fact]
        public async Task GetUserNotifications_InvalidInput_ReturnsBadRequest()
        {
           
            var ctrl = NewController();

            // Act
            var result = await ctrl.GetUserNotifications(0, 1, 10);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("phải lớn hơn 0", bad.Value!.ToString());

            _svc.Verify(s => s.GetNotificationsByUserAsync(
                            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
                        Times.Never);
        }
        [Fact]
        public async Task GetUserNotifications_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            int userId = 99;
            int pageNumber = 1;
            int pageSize = 10;

            _svc.Setup(s => s.GetNotificationsByUserAsync(userId, pageNumber, pageSize))
                .ThrowsAsync(new KeyNotFoundException("User not found"));

            var ctrl = NewController();

            // Act
            var result = await ctrl.GetUserNotifications(userId, pageNumber, pageSize);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Không tìm thấy user", notFound.Value!.ToString());
            _svc.VerifyAll();
        }
        [Fact]
        public async Task GetUserNotifications_ServiceThrows_Returns500()
        {
            // Arrange
            int userId = 7;
            int pageNumber = 1;
            int pageSize = 10;

            _svc.Setup(s => s.GetNotificationsByUserAsync(userId, pageNumber, pageSize))
                .ThrowsAsync(new Exception("Database failure"));

            var ctrl = NewController();

            // Act
            var result = await ctrl.GetUserNotifications(userId, pageNumber, pageSize);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            Assert.Contains("Có lỗi xảy ra khi lấy danh sách thông báo của user",
                            obj.Value!.ToString());
            _svc.VerifyAll();
        }

        #endregion

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
        [Fact]
        public async Task MarkAsRead_BadRequest_WhenIdsInvalid()
        {
            var ctrl = NewController();

            var result = await ctrl.MarkAsRead(0, -1);

            var badReq = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("phải > 0", badReq.Value!.ToString());

            
            _svc.Verify(s => s.MarkAsReadAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async Task MarkAsRead_Returns500_WhenServiceThrows()
        {
            _svc.Setup(s => s.MarkAsReadAsync(7, 5))
                .ThrowsAsync(new Exception("DB error"));

            var ctrl = NewController();

            var result = await ctrl.MarkAsRead(7, 5);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
        }

        [Fact]
        public async Task MarkAsRead_Ok_WhenIdsAtLowerBoundary()
        {
           
            _svc.Setup(s => s.MarkAsReadAsync(1, 1)).ReturnsAsync(true);
            var ctrl = NewController();

            var result = await ctrl.MarkAsRead(1, 1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("đánh dấu thông báo là đã đọc", ok.Value!.ToString(), StringComparison.OrdinalIgnoreCase);
            _svc.VerifyAll();
        }

        #endregion
        #region GetUnreadCount 
        [Fact]
        public async Task GetUnreadCount_ReturnsOk_WhenUserExists()
        {
   
            _svc.Setup(s => s.CountUnreadAsync(8)).ReturnsAsync(42);
            var ctrl = NewController();

           
            var result = await ctrl.GetUnreadCount(8);

        
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(42, ok.Value);   
            _svc.VerifyAll();
        }
        [Fact]
        public async Task GetUnreadCount_WithInvalidUserId_ReturnsBadRequest()
        {
            var ctrl = NewController();

            var result = await ctrl.GetUnreadCount(0);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("userId phải lớn hơn 0", bad.Value!.ToString());

            // Service KHÔNG được gọi
            _svc.Verify(s => s.CountUnreadAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetUnreadCount_ReturnsNotFound_WhenUserDoesNotExist()
        {
          
            int userId = 99;
            _svc.Setup(s => s.CountUnreadAsync(userId))
                .ThrowsAsync(new KeyNotFoundException("User not found"));

            var ctrl = NewController();
            var result = await ctrl.GetUnreadCount(userId);

      
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Không tìm thấy user", notFound.Value!.ToString());
            _svc.VerifyAll();
        }
        [Fact]
        public async Task GetUnreadCount_Returns500_WhenServiceThrows()
        {
            _svc.Setup(s => s.CountUnreadAsync(8))
                .ThrowsAsync(new Exception("Database connection failed"));

            var ctrl = NewController();

            var result = await ctrl.GetUnreadCount(8);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            Assert.Contains("Có lỗi xảy ra khi lấy số lượng thông báo chưa đọc", obj.Value!.ToString());
            _svc.VerifyAll();
        }



        #endregion
        #region MarkAllAsRead

        [Fact]
        public async Task MarkAllAsRead_Success_ReturnsNoContent()
        {
            _svc.Setup(s => s.MarkAllAsReadAsync(7))
                .Returns(Task.CompletedTask);

            var ctrl = NewController();

            
            var result = await ctrl.MarkAllAsRead(7);

            Assert.IsType<NoContentResult>(result);
            _svc.VerifyAll();
        }
        [Fact]
        public async Task MarkAllAsRead_InvalidUserId_ReturnsBadRequest()
        {
            
            var ctrl = NewController();

            var result = await ctrl.MarkAllAsRead(0);

            
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("userId must be greater than 0", bad.Value!.ToString());

           
            _svc.Verify(s => s.MarkAllAsReadAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async Task MarkAllAsRead_UserNotFound_ReturnsNotFound()
        {
           
            _svc.Setup(s => s.MarkAllAsReadAsync(99))
                .ThrowsAsync(new KeyNotFoundException("User not found"));

            var ctrl = NewController();

           
            var result = await ctrl.MarkAllAsRead(99);

          
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Không tìm thấy user", notFound.Value!.ToString());
            _svc.VerifyAll();
        }
        [Fact]
        public async Task MarkAllAsRead_ServiceThrows_Returns500()
        {
            
            _svc.Setup(s => s.MarkAllAsReadAsync(7))
                .ThrowsAsync(new Exception("Database failure"));

            var ctrl = NewController();

          
            var result = await ctrl.MarkAllAsRead(7);

            
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            Assert.Contains("Có lỗi xảy ra khi cập nhật trạng thái thông báo", obj.Value!.ToString());
            _svc.VerifyAll();
        }

        #endregion

        #region GetAllNotifications
        [Fact]
        public async Task GetAllNotifications_Success_ReturnsOkWithPagedResult()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;

            var items = new List<NotificationDTO>
    {
        new NotificationDTO
        {
            NotificationId = 1,
            Title = "N1",
            Content = "Content 1",
            Type = "System",
            CreatedDate = DateTime.UtcNow,
            IsRead = false
        },
        new NotificationDTO
        {
            NotificationId = 2,
            Title = "N2",
            Content = "Content 2",
            Type = "Reminder",
            CreatedDate = DateTime.UtcNow,
            IsRead = true
        }
    };

            var paged = new PaginationHelper.PagedResult<NotificationDTO>
            {
                Items = items,
                TotalCount = 2,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _svc.Setup(s => s.GetListNotificationsAsync(pageNumber, pageSize))
                .ReturnsAsync(paged);

            var ctrl = NewController();

            // Act
            var result = await ctrl.GetAllNotifications(pageNumber, pageSize);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<PaginationHelper.PagedResult<NotificationDTO>>(ok.Value);

            Assert.Equal(2, value.TotalCount);
            Assert.Equal(pageNumber, value.PageNumber);
            Assert.Equal(pageSize, value.PageSize);
            Assert.Equal(2, value.Items.Count);
            Assert.Equal("N1", value.Items[0].Title);

            _svc.VerifyAll();
        }
        [Fact]
        public async Task GetAllNotifications_InvalidPaging_ReturnsBadRequest()
        {
            var ctrl = NewController();

            // pageNumber <= 0
            var result = await ctrl.GetAllNotifications(0, 10);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("pageNumber và pageSize phải lớn hơn 0", bad.Value!.ToString());

            _svc.Verify(s => s.GetListNotificationsAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async Task GetAllNotifications_ServiceThrows_Returns500()
        {
            _svc.Setup(s => s.GetListNotificationsAsync(1, 10))
                .ThrowsAsync(new Exception("Database failure"));

            var ctrl = NewController();

            var result = await ctrl.GetAllNotifications(1, 10);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            Assert.Contains("Có lỗi xảy ra khi lấy danh sách thông báo", obj.Value!.ToString());

            _svc.VerifyAll();
        }



        #endregion

        #region GetAll (all-user endpoint)

        [Fact]
        public async Task GetAll_ReturnsOk_WithUsers()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { UserId = 1, FullName = "User 1", Phone = "0909123456" },
                new UserDto { UserId = 2, FullName = "User 2", Phone = "0909123457" }
            };
            _adminSvc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<UserDto>)users);

            var ctrl = NewController();

            // Act
            var result = await ctrl.GetAll(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<UserDto>>(ok.Value);
            var list = value.ToList();
            Assert.Equal(2, list.Count);
            Assert.Equal("User 1", list[0].FullName);
            _adminSvc.VerifyAll();
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithEmptyList()
        {
            // Arrange
            _adminSvc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<UserDto>)new List<UserDto>());

            var ctrl = NewController();

            // Act
            var result = await ctrl.GetAll(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<UserDto>>(ok.Value);
            Assert.Empty(value);
            _adminSvc.VerifyAll();
        }
        #endregion
    }
}
