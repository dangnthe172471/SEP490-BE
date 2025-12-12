using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.ManageReceptionist.ManageAppointment;

namespace SEP490_BE.Tests.Controllers
{
    public class PatientControllerTests
    {
        private readonly Mock<IPatientService> _serviceMock = new();

        [Fact]
        public async Task GetById_WithValidId_ReturnsOk()
        {
            // Arrange
            var patient = new PatientInfoDto
            {
                PatientId = 1,
                FullName = "John Doe",
                Phone = "0905123456"
            };
            _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            var controller = new PatientController(_serviceMock.Object);

            // Act
            var result = await controller.GetById(1, CancellationToken.None);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var data = okResult!.Value as PatientInfoDto;
            data!.PatientId.Should().Be(1);
            data.FullName.Should().Be("John Doe");
        }

        [Fact]
        public async Task GetById_WithNotFound_ReturnsNotFound()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PatientInfoDto?)null);

            var controller = new PatientController(_serviceMock.Object);

            // Act
            var result = await controller.GetById(999, CancellationToken.None);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetById_WithZeroId_ReturnsNotFound()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetByIdAsync(0, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PatientInfoDto?)null);

            var controller = new PatientController(_serviceMock.Object);

            // Act
            var result = await controller.GetById(0, CancellationToken.None);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }
    }
}



