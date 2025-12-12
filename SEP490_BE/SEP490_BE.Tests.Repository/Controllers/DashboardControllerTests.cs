using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers.Dashboard;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.Dashboard;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers
{
    public class DashboardControllerTests
    {
        private readonly Mock<IDashboardService> _serviceMock = new();

        private DashboardController MakeController(string? role = "Receptionist")
        {
            var controller = new DashboardController(_serviceMock.Object);
            var claims = new List<Claim>();
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
        public async Task GetClinicStatus_WithDate_ReturnsOk()
        {
            // Arrange
            var date = DateOnly.FromDateTime(DateTime.Today);
            var status = new ClinicStatusDto
            {
                Date = date,
                Appointments = new ClinicStatusDto.AppointmentCounters
                {
                    Total = 10,
                    Completed = 5,
                    Pending = 3,
                    Cancelled = 2
                }
            };
            _serviceMock.Setup(s => s.GetClinicStatusAsync(date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(status);

            var controller = MakeController("Receptionist");

            // Act
            var result = await controller.GetClinicStatus(date, CancellationToken.None);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var data = okResult!.Value as ClinicStatusDto;
            data!.Appointments.Total.Should().Be(10);
        }

        [Fact]
        public async Task GetClinicStatus_WithoutDate_ReturnsOkWithToday()
        {
            // Arrange
            var today = DateOnly.FromDateTime(DateTime.Today);
            var status = new ClinicStatusDto 
            { 
                Date = today,
                Appointments = new ClinicStatusDto.AppointmentCounters { Total = 5 }
            };
            _serviceMock.Setup(s => s.GetClinicStatusAsync(today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(status);

            var controller = MakeController("Receptionist");

            // Act
            var result = await controller.GetClinicStatus(null, CancellationToken.None);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPatientStatistics_WithValidRange_ReturnsOk()
        {
            // Arrange
            var from = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
            var to = DateOnly.FromDateTime(DateTime.Today);
            var stats = new PatientStatisticsDto
            {
                TotalPatients = 100
            };
            _serviceMock.Setup(s => s.GetPatientStatisticsAsync(from, to, It.IsAny<CancellationToken>()))
                .ReturnsAsync(stats);

            var controller = MakeController("Clinic Manager");

            // Act
            var result = await controller.GetPatientStatistics(from, to, CancellationToken.None);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPatientStatistics_WithInvalidRange_ReturnsBadRequest()
        {
            // Arrange
            var from = DateOnly.FromDateTime(DateTime.Today);
            var to = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

            var controller = MakeController("Clinic Manager");

            // Act
            var result = await controller.GetPatientStatistics(from, to, CancellationToken.None);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetPatientStatistics_WithoutDates_UsesDefaults()
        {
            // Arrange
            var to = DateOnly.FromDateTime(DateTime.Today);
            var from = to.AddMonths(-11).AddDays(1 - to.Day);
            var stats = new PatientStatisticsDto { TotalPatients = 50 };
            _serviceMock.Setup(s => s.GetPatientStatisticsAsync(from, to, It.IsAny<CancellationToken>()))
                .ReturnsAsync(stats);

            var controller = MakeController("Clinic Manager");

            // Act
            var result = await controller.GetPatientStatistics(null, null, CancellationToken.None);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetTestDiagnosticStats_WithValidParams_ReturnsOk()
        {
            // Arrange
            var from = DateOnly.FromDateTime(DateTime.Today.AddDays(-29));
            var to = DateOnly.FromDateTime(DateTime.Today);
            var stats = new TestDiagnosticStatsDto
            {
                TotalTests = 50,
                TotalVisits = 30
            };
            _serviceMock.Setup(s => s.GetTestDiagnosticStatsAsync(from, to, "day", It.IsAny<CancellationToken>()))
                .ReturnsAsync(stats);

            var controller = MakeController("Clinic Manager");

            // Act
            var result = await controller.GetTestDiagnosticStats(from, to, "day", CancellationToken.None);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetTestDiagnosticStats_WithMonthGroupBy_ReturnsOk()
        {
            // Arrange
            var from = DateOnly.FromDateTime(DateTime.Today.AddDays(-29));
            var to = DateOnly.FromDateTime(DateTime.Today);
            var stats = new TestDiagnosticStatsDto { TotalTests = 100 };
            _serviceMock.Setup(s => s.GetTestDiagnosticStatsAsync(from, to, "month", It.IsAny<CancellationToken>()))
                .ReturnsAsync(stats);

            var controller = MakeController("Clinic Manager");

            // Act
            var result = await controller.GetTestDiagnosticStats(from, to, "month", CancellationToken.None);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetTestDiagnosticStats_WithInvalidRange_ReturnsBadRequest()
        {
            // Arrange
            var from = DateOnly.FromDateTime(DateTime.Today);
            var to = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

            var controller = MakeController("Clinic Manager");

            // Act
            var result = await controller.GetTestDiagnosticStats(from, to, "day", CancellationToken.None);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetTestDiagnosticStats_WithoutDates_UsesDefaults()
        {
            // Arrange
            var to = DateOnly.FromDateTime(DateTime.Today);
            var from = to.AddDays(-29);
            var stats = new TestDiagnosticStatsDto { TotalTests = 25 };
            _serviceMock.Setup(s => s.GetTestDiagnosticStatsAsync(from, to, "day", It.IsAny<CancellationToken>()))
                .ReturnsAsync(stats);

            var controller = MakeController("Clinic Manager");

            // Act
            var result = await controller.GetTestDiagnosticStats(null, null, null, CancellationToken.None);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetTestDiagnosticStats_WithInvalidGroupBy_DefaultsToDay()
        {
            // Arrange
            var from = DateOnly.FromDateTime(DateTime.Today.AddDays(-29));
            var to = DateOnly.FromDateTime(DateTime.Today);
            var stats = new TestDiagnosticStatsDto { TotalTests = 30 };
            _serviceMock.Setup(s => s.GetTestDiagnosticStatsAsync(from, to, "day", It.IsAny<CancellationToken>()))
                .ReturnsAsync(stats);

            var controller = MakeController("Clinic Manager");

            // Act
            var result = await controller.GetTestDiagnosticStats(from, to, "invalid", CancellationToken.None);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }
    }
}

