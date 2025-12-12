using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers.DoctorControllers;
using SEP490_BE.BLL.IServices.IDoctorServices;
using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.Tests.Controllers
{
    public class DoctorScheduleControllerTests
    {
        private readonly Mock<IDoctorScheduleService> _serviceMock = new();

        [Fact]
        public async Task GetDoctorActiveScheduleInRange_WithValidParams_ReturnsOk()
        {
            // Arrange
            var doctorId = 1;
            var startDate = DateOnly.FromDateTime(DateTime.Today);
            var endDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
            var schedules = new List<DoctorActiveScheduleRangeDto>
            {
                new DoctorActiveScheduleRangeDto { DoctorId = doctorId, Date = startDate, DoctorName = "Dr. Test", Specialty = "Cardiology", RoomName = "Room 1", ShiftType = "Morning", StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(12, 0) },
                new DoctorActiveScheduleRangeDto { DoctorId = doctorId, Date = startDate.AddDays(1), DoctorName = "Dr. Test", Specialty = "Cardiology", RoomName = "Room 1", ShiftType = "Afternoon", StartTime = new TimeOnly(13, 0), EndTime = new TimeOnly(17, 0) }
            };
            _serviceMock.Setup(s => s.GetDoctorActiveScheduleInRangeAsync(doctorId, startDate, endDate))
                .ReturnsAsync(schedules);

            var controller = new DoctorScheduleController(_serviceMock.Object);

            // Act
            var result = await controller.GetDoctorActiveScheduleInRange(doctorId, startDate, endDate);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDoctorActiveScheduleInRange_WithNullDates_UsesDefaults()
        {
            // Arrange
            var doctorId = 1;
            var schedules = new List<DoctorActiveScheduleRangeDto>();
            _serviceMock.Setup(s => s.GetDoctorActiveScheduleInRangeAsync(doctorId, default, default))
                .ReturnsAsync(schedules);

            var controller = new DoctorScheduleController(_serviceMock.Object);

            // Act
            var result = await controller.GetDoctorActiveScheduleInRange(doctorId, null, null);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var message = okResult!.Value?.GetType().GetProperty("message")?.GetValue(okResult.Value)?.ToString();
            message.Should().Contain("Không có lịch làm việc");
        }

        [Fact]
        public async Task GetDoctorActiveScheduleInRange_WithEmptyResult_ReturnsOkWithMessage()
        {
            // Arrange
            var doctorId = 1;
            var startDate = DateOnly.FromDateTime(DateTime.Today);
            var endDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
            _serviceMock.Setup(s => s.GetDoctorActiveScheduleInRangeAsync(doctorId, startDate, endDate))
                .ReturnsAsync(new List<DoctorActiveScheduleRangeDto>());

            var controller = new DoctorScheduleController(_serviceMock.Object);

            // Act
            var result = await controller.GetDoctorActiveScheduleInRange(doctorId, startDate, endDate);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var message = okResult!.Value?.GetType().GetProperty("message")?.GetValue(okResult.Value)?.ToString();
            message.Should().Contain("Không có lịch làm việc");
        }

        [Fact]
        public async Task GetDoctorActiveScheduleInRange_WithNullResult_ReturnsOkWithMessage()
        {
            // Arrange
            var doctorId = 1;
            var startDate = DateOnly.FromDateTime(DateTime.Today);
            var endDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
            _serviceMock.Setup(s => s.GetDoctorActiveScheduleInRangeAsync(doctorId, startDate, endDate))
                .ReturnsAsync((List<DoctorActiveScheduleRangeDto>?)null);

            var controller = new DoctorScheduleController(_serviceMock.Object);

            // Act
            var result = await controller.GetDoctorActiveScheduleInRange(doctorId, startDate, endDate);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var message = okResult!.Value?.GetType().GetProperty("message")?.GetValue(okResult.Value)?.ToString();
            message.Should().Contain("Không có lịch làm việc");
        }

        [Fact]
        public async Task GetDoctorActiveScheduleInRange_WithArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var doctorId = 1;
            var startDate = DateOnly.FromDateTime(DateTime.Today);
            var endDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)); // Invalid: endDate < startDate
            _serviceMock.Setup(s => s.GetDoctorActiveScheduleInRangeAsync(doctorId, startDate, endDate))
                .ThrowsAsync(new ArgumentException("Invalid date range"));

            var controller = new DoctorScheduleController(_serviceMock.Object);

            // Act
            var result = await controller.GetDoctorActiveScheduleInRange(doctorId, startDate, endDate);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetDoctorActiveScheduleInRange_WithException_ReturnsInternalServerError()
        {
            // Arrange
            var doctorId = 1;
            var startDate = DateOnly.FromDateTime(DateTime.Today);
            var endDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
            _serviceMock.Setup(s => s.GetDoctorActiveScheduleInRangeAsync(doctorId, startDate, endDate))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new DoctorScheduleController(_serviceMock.Object);

            // Act
            var result = await controller.GetDoctorActiveScheduleInRange(doctorId, startDate, endDate);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetDoctorActiveScheduleInRange_WithValidData_ReturnsData()
        {
            // Arrange
            var doctorId = 1;
            var startDate = DateOnly.FromDateTime(DateTime.Today);
            var endDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
            var schedules = new List<DoctorActiveScheduleRangeDto>
            {
                new DoctorActiveScheduleRangeDto { DoctorId = doctorId, Date = startDate, DoctorName = "Dr. Test", Specialty = "Cardiology", RoomName = "Room 1", ShiftType = "Morning", StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(12, 0) },
                new DoctorActiveScheduleRangeDto { DoctorId = doctorId, Date = startDate.AddDays(1), DoctorName = "Dr. Test", Specialty = "Cardiology", RoomName = "Room 1", ShiftType = "Afternoon", StartTime = new TimeOnly(13, 0), EndTime = new TimeOnly(17, 0) }
            };
            _serviceMock.Setup(s => s.GetDoctorActiveScheduleInRangeAsync(doctorId, startDate, endDate))
                .ReturnsAsync(schedules);

            var controller = new DoctorScheduleController(_serviceMock.Object);

            // Act
            var result = await controller.GetDoctorActiveScheduleInRange(doctorId, startDate, endDate);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var data = okResult!.Value as List<DoctorActiveScheduleRangeDto>;
            data.Should().HaveCount(2);
        }
    }
}

