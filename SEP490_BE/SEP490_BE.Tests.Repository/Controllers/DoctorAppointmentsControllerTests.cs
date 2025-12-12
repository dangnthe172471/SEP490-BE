using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.AppointmentDTO;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers
{
    public class DoctorAppointmentsControllerTests
    {
        private readonly Mock<IAppointmentDoctorService> _serviceMock = new();
        private DoctorAppointmentsController MakeController(int? userId = 1, string? role = "Doctor")
        {
            var controller = new DoctorAppointmentsController(_serviceMock.Object);
            var claims = new List<Claim>();
            if (userId.HasValue)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
            if (!string.IsNullOrEmpty(role))
                claims.Add(new Claim(ClaimTypes.Role, role));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"))
                }
            };
            return controller;
        }

        [Fact]
        public async Task GetMyAppointments_WithValidUserId_ReturnsOk()
        {
            // Arrange
            var appointments = new List<AppointmentListItemDto>
            {
                new() { AppointmentId = 1, PatientName = "John Doe" },
                new() { AppointmentId = 2, PatientName = "Jane Doe" }
            };
            _serviceMock.Setup(s => s.GetDoctorAppointmentsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointments);

            var controller = MakeController(1, "Doctor");

            // Act
            var result = await controller.GetMyAppointments();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var data = okResult!.Value as List<AppointmentListItemDto>;
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetMyAppointments_WithInvalidUserId_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_serviceMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };

            // Act
            var result = await controller.GetMyAppointments();

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task GetMyAppointmentDetail_WithValidId_ReturnsOk()
        {
            // Arrange
            var appointment = new AppointmentDetailDto
            {
                AppointmentId = 1,
                PatientName = "John Doe",
                VisitReason = "Checkup"
            };
            _serviceMock.Setup(s => s.GetDoctorAppointmentDetailAsync(1, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            var controller = MakeController(1, "Doctor");

            // Act
            var result = await controller.GetMyAppointmentDetail(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMyAppointmentDetail_WithNotFound_ReturnsNotFound()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetDoctorAppointmentDetailAsync(1, 999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((AppointmentDetailDto?)null);

            var controller = MakeController(1, "Doctor");

            // Act
            var result = await controller.GetMyAppointmentDetail(999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetMyAppointmentDetail_WithInvalidUserId_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_serviceMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };

            // Act
            var result = await controller.GetMyAppointmentDetail(1);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task GetMyAppointments_WithEmptyList_ReturnsOk()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetDoctorAppointmentsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AppointmentListItemDto>());

            var controller = MakeController(1, "Doctor");

            // Act
            var result = await controller.GetMyAppointments();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var data = okResult!.Value as List<AppointmentListItemDto>;
            data.Should().BeEmpty();
        }
    }
}

