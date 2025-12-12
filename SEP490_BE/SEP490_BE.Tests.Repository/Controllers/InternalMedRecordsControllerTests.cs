using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.InternalMedRecordsDTO;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers
{
    public class InternalMedRecordsControllerTests
    {
        private readonly Mock<IInternalMedRecordService> _serviceMock = new(MockBehavior.Strict);

        private InternalMedRecordsController CreateController(string? role = "Doctor")
        {
            var controller = new InternalMedRecordsController(_serviceMock.Object);
            var claims = new List<Claim>();
            if (!string.IsNullOrEmpty(role))
                claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, "1"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"))
                }
            };
            return controller;
        }

        #region Get Tests

        [Fact]
        public async Task Get_WithValidRecordId_ReturnsOk()
        {
            // Arrange
            var recordId = 1;
            var expectedDto = new ReadInternalMedRecordDto
            {
                RecordId = recordId,
                BloodPressure = 120,
                HeartRate = 72,
                BloodSugar = 5.5m
            };

            _serviceMock.Setup(s => s.GetByRecordIdAsync(recordId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var controller = CreateController("Doctor");

            // Act
            var result = await controller.Get(recordId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<ReadInternalMedRecordDto>(okResult.Value);
            value.RecordId.Should().Be(recordId);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task Get_WithNonExistentRecordId_ReturnsNotFound()
        {
            // Arrange
            var recordId = 999;
            _serviceMock.Setup(s => s.GetByRecordIdAsync(recordId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReadInternalMedRecordDto?)null);

            var controller = CreateController("Patient");

            // Act
            var result = await controller.Get(recordId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.Value.Should().NotBeNull();
            _serviceMock.VerifyAll();
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_WithValidDto_ReturnsCreated()
        {
            // Arrange
            var dto = new CreateInternalMedRecordDto
            {
                RecordId = 1,
                BloodPressure = 120,
                HeartRate = 72,
                BloodSugar = 5.5m
            };

            var createdDto = new ReadInternalMedRecordDto
            {
                RecordId = 1,
                BloodPressure = 120,
                HeartRate = 72,
                BloodSugar = 5.5m
            };

            _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdDto);

            var controller = CreateController("Doctor");

            // Act
            var result = await controller.Create(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsType<ReadInternalMedRecordDto>(createdResult.Value);
            value.RecordId.Should().Be(1);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task Create_WithKeyNotFoundException_ReturnsNotFound()
        {
            // Arrange
            var dto = new CreateInternalMedRecordDto { RecordId = 999 };
            _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Không tìm thấy phiếu khám"));

            var controller = CreateController("Doctor");

            // Act
            var result = await controller.Create(dto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task Create_WithInvalidOperationException_ReturnsConflict()
        {
            // Arrange
            var dto = new CreateInternalMedRecordDto { RecordId = 1 };
            _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Hồ sơ đã tồn tại"));

            var controller = CreateController("Doctor");

            // Act
            var result = await controller.Create(dto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task Create_WithException_ReturnsInternalServerError()
        {
            // Arrange
            var dto = new CreateInternalMedRecordDto { RecordId = 1 };
            _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var controller = CreateController("Doctor");

            // Act
            var result = await controller.Create(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            objectResult.StatusCode.Should().Be(500);
            _serviceMock.VerifyAll();
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_WithValidData_ReturnsOk()
        {
            // Arrange
            var recordId = 1;
            var dto = new UpdateInternalMedRecordDto
            {
                BloodPressure = 130,
                HeartRate = 75,
                BloodSugar = 6.0m
            };

            var updatedDto = new ReadInternalMedRecordDto
            {
                RecordId = recordId,
                BloodPressure = 130,
                HeartRate = 75,
                BloodSugar = 6.0m
            };

            _serviceMock.Setup(s => s.UpdateAsync(recordId, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedDto);

            var controller = CreateController("Doctor");

            // Act
            var result = await controller.Update(recordId, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<ReadInternalMedRecordDto>(okResult.Value);
            value.RecordId.Should().Be(recordId);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task Update_WithKeyNotFoundException_ReturnsNotFound()
        {
            // Arrange
            var recordId = 999;
            var dto = new UpdateInternalMedRecordDto();
            _serviceMock.Setup(s => s.UpdateAsync(recordId, dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Không tìm thấy hồ sơ"));

            var controller = CreateController("Nurse");

            // Act
            var result = await controller.Update(recordId, dto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task Update_WithException_ReturnsInternalServerError()
        {
            // Arrange
            var recordId = 1;
            var dto = new UpdateInternalMedRecordDto();
            _serviceMock.Setup(s => s.UpdateAsync(recordId, dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var controller = CreateController("Doctor");

            // Act
            var result = await controller.Update(recordId, dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            objectResult.StatusCode.Should().Be(500);
            _serviceMock.VerifyAll();
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidRecordId_ReturnsNoContent()
        {
            // Arrange
            var recordId = 1;
            _serviceMock.Setup(s => s.DeleteAsync(recordId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var controller = CreateController("Doctor");

            // Act
            var result = await controller.Delete(recordId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task Delete_WithException_ReturnsInternalServerError()
        {
            // Arrange
            var recordId = 1;
            _serviceMock.Setup(s => s.DeleteAsync(recordId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var controller = CreateController("Doctor");

            // Act
            var result = await controller.Delete(recordId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            objectResult.StatusCode.Should().Be(500);
            _serviceMock.VerifyAll();
        }

        #endregion

        #region GetSpecialtyStatus Tests

        [Fact]
        public async Task GetSpecialtyStatus_WithValidRecordId_ReturnsOk()
        {
            // Arrange
            var recordId = 1;
            _serviceMock.Setup(s => s.CheckSpecialtiesAsync(recordId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, false, true));

            var controller = CreateController("Patient");

            // Act
            var result = await controller.GetSpecialtyStatus(recordId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.Value.Should().NotBeNull();
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task GetSpecialtyStatus_WithKeyNotFoundException_ReturnsNotFound()
        {
            // Arrange
            var recordId = 999;
            _serviceMock.Setup(s => s.CheckSpecialtiesAsync(recordId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Không tìm thấy hồ sơ"));

            var controller = CreateController("Doctor");

            // Act
            var result = await controller.GetSpecialtyStatus(recordId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            _serviceMock.VerifyAll();
        }

        #endregion
    }
}

