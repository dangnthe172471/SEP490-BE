using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.DoctorStatisticsDTO;

namespace SEP490_BE.Tests.Controllers
{
    public class DoctorStatisticsControllerTests
    {
        private readonly Mock<IDoctorStatisticsService> _serviceMock = new(MockBehavior.Strict);

        private DoctorStatisticsController CreateController()
        {
            return new DoctorStatisticsController(_serviceMock.Object);
        }

        #region GetPatientCountByDoctor Tests

        [Fact]
        public async Task GetPatientCountByDoctor_WithValidDates_ReturnsOk()
        {
            // Arrange
            var fromDate = new DateTime(2025, 1, 1);
            var toDate = new DateTime(2025, 1, 31);
            var expectedData = new List<DoctorPatientCountDto>
            {
                new DoctorPatientCountDto { DoctorId = 1, DoctorName = "Dr. A", Specialty = "Cardiology", TotalPatients = 10, TotalAppointments = 15 },
                new DoctorPatientCountDto { DoctorId = 2, DoctorName = "Dr. B", Specialty = "Dermatology", TotalPatients = 8, TotalAppointments = 12 }
            };

            _serviceMock.Setup(s => s.GetPatientCountByDoctorAsync(fromDate, toDate))
                .ReturnsAsync(expectedData);

            var controller = CreateController();

            // Act
            var result = await controller.GetPatientCountByDoctor(fromDate, toDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<List<DoctorPatientCountDto>>(okResult.Value);
            value.Should().HaveCount(2);
            value[0].DoctorId.Should().Be(1);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task GetPatientCountByDoctor_WithEmptyResult_ReturnsOk()
        {
            // Arrange
            var fromDate = new DateTime(2025, 1, 1);
            var toDate = new DateTime(2025, 1, 31);
            var expectedData = new List<DoctorPatientCountDto>();

            _serviceMock.Setup(s => s.GetPatientCountByDoctorAsync(fromDate, toDate))
                .ReturnsAsync(expectedData);

            var controller = CreateController();

            // Act
            var result = await controller.GetPatientCountByDoctor(fromDate, toDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<List<DoctorPatientCountDto>>(okResult.Value);
            value.Should().BeEmpty();
            _serviceMock.VerifyAll();
        }

        #endregion

        #region GetDoctorVisitTrend Tests

        [Fact]
        public async Task GetDoctorVisitTrend_WithValidDates_ReturnsOk()
        {
            // Arrange
            var fromDate = new DateTime(2025, 1, 1);
            var toDate = new DateTime(2025, 1, 31);
            var expectedData = new List<DoctorVisitTrendPointDto>
            {
                new DoctorVisitTrendPointDto { DoctorId = 1, DoctorName = "Dr. A", Date = new DateTime(2025, 1, 1), VisitCount = 5 },
                new DoctorVisitTrendPointDto { DoctorId = 1, DoctorName = "Dr. A", Date = new DateTime(2025, 1, 2), VisitCount = 8 }
            };

            _serviceMock.Setup(s => s.GetDoctorVisitTrendAsync(fromDate, toDate, null))
                .ReturnsAsync(expectedData);

            var controller = CreateController();

            // Act
            var result = await controller.GetDoctorVisitTrend(fromDate, toDate, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<List<DoctorVisitTrendPointDto>>(okResult.Value);
            value.Should().HaveCount(2);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task GetDoctorVisitTrend_WithDoctorId_ReturnsOk()
        {
            // Arrange
            var fromDate = new DateTime(2025, 1, 1);
            var toDate = new DateTime(2025, 1, 31);
            var doctorId = 1;
            var expectedData = new List<DoctorVisitTrendPointDto>
            {
                new DoctorVisitTrendPointDto { DoctorId = doctorId, DoctorName = "Dr. A", Date = new DateTime(2025, 1, 1), VisitCount = 3 }
            };

            _serviceMock.Setup(s => s.GetDoctorVisitTrendAsync(fromDate, toDate, doctorId))
                .ReturnsAsync(expectedData);

            var controller = CreateController();

            // Act
            var result = await controller.GetDoctorVisitTrend(fromDate, toDate, doctorId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<List<DoctorVisitTrendPointDto>>(okResult.Value);
            value.Should().HaveCount(1);
            _serviceMock.VerifyAll();
        }

        #endregion

        #region GetDoctorReturnRates Tests

        [Fact]
        public async Task GetDoctorReturnRates_WithValidDates_ReturnsOk()
        {
            // Arrange
            var fromDate = new DateTime(2025, 1, 1);
            var toDate = new DateTime(2025, 1, 31);
            var expectedData = new List<DoctorReturnRateDto>
            {
                new DoctorReturnRateDto { DoctorId = 1, DoctorName = "Dr. A", ReturnRate = 0.75 },
                new DoctorReturnRateDto { DoctorId = 2, DoctorName = "Dr. B", ReturnRate = 0.60 }
            };

            _serviceMock.Setup(s => s.GetDoctorReturnRatesAsync(fromDate, toDate))
                .ReturnsAsync(expectedData);

            var controller = CreateController();

            // Act
            var result = await controller.GetDoctorReturnRates(fromDate, toDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<List<DoctorReturnRateDto>>(okResult.Value);
            value.Should().HaveCount(2);
            value[0].ReturnRate.Should().Be(0.75);
            _serviceMock.VerifyAll();
        }

        #endregion

        #region GetDoctorStatisticsSummary Tests

        [Fact]
        public async Task GetDoctorStatisticsSummary_WithValidDates_ReturnsOk()
        {
            // Arrange
            var fromDate = new DateTime(2025, 1, 1);
            var toDate = new DateTime(2025, 1, 31);
            var expectedData = new DoctorStatisticsSummaryDto
            {
                PatientCountByDoctor = new List<DoctorPatientCountDto>
                {
                    new DoctorPatientCountDto { DoctorId = 1, DoctorName = "Dr. A", Specialty = "Cardiology", TotalPatients = 10, TotalAppointments = 15 }
                },
                VisitTrend = new List<DoctorVisitTrendPointDto>
                {
                    new DoctorVisitTrendPointDto { DoctorId = 1, DoctorName = "Dr. A", Date = new DateTime(2025, 1, 1), VisitCount = 5 }
                },
                ReturnRates = new List<DoctorReturnRateDto>
                {
                    new DoctorReturnRateDto { DoctorId = 1, DoctorName = "Dr. A", ReturnRate = 0.75 }
                }
            };

            _serviceMock.Setup(s => s.GetDoctorStatisticsSummaryAsync(fromDate, toDate, null))
                .ReturnsAsync(expectedData);

            var controller = CreateController();

            // Act
            var result = await controller.GetDoctorStatisticsSummary(fromDate, toDate, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<DoctorStatisticsSummaryDto>(okResult.Value);
            value.PatientCountByDoctor.Should().HaveCount(1);
            value.VisitTrend.Should().HaveCount(1);
            value.ReturnRates.Should().HaveCount(1);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task GetDoctorStatisticsSummary_WithDoctorId_ReturnsOk()
        {
            // Arrange
            var fromDate = new DateTime(2025, 1, 1);
            var toDate = new DateTime(2025, 1, 31);
            var doctorId = 1;
            var expectedData = new DoctorStatisticsSummaryDto
            {
                PatientCountByDoctor = new List<DoctorPatientCountDto>(),
                VisitTrend = new List<DoctorVisitTrendPointDto>(),
                ReturnRates = new List<DoctorReturnRateDto>()
            };

            _serviceMock.Setup(s => s.GetDoctorStatisticsSummaryAsync(fromDate, toDate, doctorId))
                .ReturnsAsync(expectedData);

            var controller = CreateController();

            // Act
            var result = await controller.GetDoctorStatisticsSummary(fromDate, toDate, doctorId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<DoctorStatisticsSummaryDto>(okResult.Value);
            value.Should().NotBeNull();
            _serviceMock.VerifyAll();
        }

        #endregion
    }
}

