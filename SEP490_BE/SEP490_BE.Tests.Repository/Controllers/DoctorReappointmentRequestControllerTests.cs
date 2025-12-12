using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers.DoctorControllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers
{
    public class DoctorReappointmentRequestControllerTests
    {
        private readonly Mock<IReappointmentRequestService> _serviceMock = new(MockBehavior.Strict);

        private ReappointmentRequestController CreateController(int userId = 1)
        {
            var controller = new ReappointmentRequestController(_serviceMock.Object);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "Doctor")
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"))
                }
            };
            return controller;
        }

        #region CreateReappointmentRequest Tests

        [Fact]
        public async Task CreateReappointmentRequest_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var userId = 1;
            var request = new CreateReappointmentRequestDto
            {
                AppointmentId = 1,
                PreferredDate = DateTime.Now.AddDays(7),
                Notes = "Follow-up check"
            };

            var notificationId = 123;
            _serviceMock.Setup(s => s.CreateReappointmentRequestAsync(request, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificationId);

            var controller = CreateController(userId);

            // Act
            var result = await controller.CreateReappointmentRequest(request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.Value.Should().NotBeNull();
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task CreateReappointmentRequest_WithInvalidUserId_ReturnsUnauthorized()
        {
            // Arrange
            var controller = CreateController(0); // userId = 0
            var request = new CreateReappointmentRequestDto();

            // Act
            var result = await controller.CreateReappointmentRequest(request, CancellationToken.None);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            _serviceMock.Verify(s => s.CreateReappointmentRequestAsync(It.IsAny<CreateReappointmentRequestDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateReappointmentRequest_WithArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            var request = new CreateReappointmentRequestDto();
            _serviceMock.Setup(s => s.CreateReappointmentRequestAsync(request, userId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Invalid request"));

            var controller = CreateController(userId);

            // Act
            var result = await controller.CreateReappointmentRequest(request, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task CreateReappointmentRequest_WithUnauthorizedAccessException_ReturnsForbid()
        {
            // Arrange
            var userId = 1;
            var request = new CreateReappointmentRequestDto();
            _serviceMock.Setup(s => s.CreateReappointmentRequestAsync(request, userId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            var controller = CreateController(userId);

            // Act
            var result = await controller.CreateReappointmentRequest(request, CancellationToken.None);

            // Assert
            var forbidResult = Assert.IsType<ForbidResult>(result.Result);
            _serviceMock.VerifyAll();
        }

        #endregion

        #region GetMyReappointmentRequests Tests

        [Fact]
        public async Task GetMyReappointmentRequests_WithValidUserId_ReturnsOk()
        {
            // Arrange
            var userId = 1;
            var expectedRequests = new List<ReappointmentRequestDto>
            {
                new ReappointmentRequestDto
                {
                    NotificationId = 1,
                    PatientName = "John Doe",
                    DoctorName = "Dr. Smith"
                }
            };

            _serviceMock.Setup(s => s.GetMyReappointmentRequestsAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedRequests);

            var controller = CreateController(userId);

            // Act
            var result = await controller.GetMyReappointmentRequests(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<List<ReappointmentRequestDto>>(okResult.Value);
            value.Should().HaveCount(1);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task GetMyReappointmentRequests_WithInvalidUserId_ReturnsUnauthorized()
        {
            // Arrange
            var controller = CreateController(0); // userId = 0

            // Act
            var result = await controller.GetMyReappointmentRequests(CancellationToken.None);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            _serviceMock.Verify(s => s.GetMyReappointmentRequestsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetMyReappointmentRequests_WithArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            _serviceMock.Setup(s => s.GetMyReappointmentRequestsAsync(userId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Invalid user"));

            var controller = CreateController(userId);

            // Act
            var result = await controller.GetMyReappointmentRequests(CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            _serviceMock.VerifyAll();
        }

        #endregion
    }
}

