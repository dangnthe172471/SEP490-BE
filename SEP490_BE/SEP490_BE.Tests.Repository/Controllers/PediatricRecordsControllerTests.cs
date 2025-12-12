using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.PediatricRecordsDTO;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers
{
    public class PediatricRecordsControllerTests
    {
        private readonly Mock<IPediatricRecordService> _serviceMock = new(MockBehavior.Strict);

        private PediatricRecordsController CreateController(string? role = "Doctor")
        {
            var controller = new PediatricRecordsController(_serviceMock.Object);
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
            var expectedDto = new ReadPediatricRecordDto
            {
                RecordId = recordId,
                WeightKg = 15.5m,
                HeightCm = 100.0m,
                HeartRate = 80,
                TemperatureC = 37.0m
            };

            _serviceMock.Setup(s => s.GetByRecordIdAsync(recordId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var controller = CreateController("Doctor");

            // Act
            var result = await controller.Get(recordId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<ReadPediatricRecordDto>(okResult.Value);
            value.RecordId.Should().Be(recordId);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task Get_WithNonExistentRecordId_ReturnsNotFound()
        {
            // Arrange
            var recordId = 999;
            _serviceMock.Setup(s => s.GetByRecordIdAsync(recordId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReadPediatricRecordDto?)null);

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
            var dto = new CreatePediatricRecordDto
            {
                RecordId = 1,
                WeightKg = 15.5m,
                HeightCm = 100.0m,
                HeartRate = 80,
                TemperatureC = 37.0m
            };

            var createdDto = new ReadPediatricRecordDto
            {
                RecordId = 1,
                WeightKg = 15.5m,
                HeightCm = 100.0m,
                HeartRate = 80,
                TemperatureC = 37.0m
            };

            _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdDto);

            var controller = CreateController("Doctor");

            // Act
            var result = await controller.Create(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsType<ReadPediatricRecordDto>(createdResult.Value);
            value.RecordId.Should().Be(1);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task Create_WithArgumentNullException_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreatePediatricRecordDto();
            _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentNullException("dto", "Dữ liệu không được để trống"));

            var controller = CreateController("Nurse");

            // Act
            var result = await controller.Create(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequestResult.Value.Should().NotBeNull();
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task Create_WithKeyNotFoundException_ReturnsNotFound()
        {
            // Arrange
            var dto = new CreatePediatricRecordDto { RecordId = 999 };
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
            var dto = new CreatePediatricRecordDto { RecordId = 1 };
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
            var dto = new CreatePediatricRecordDto { RecordId = 1 };
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
            var dto = new UpdatePediatricRecordDto
            {
                WeightKg = 16.0m,
                HeightCm = 105.0m,
                HeartRate = 85
            };

            var updatedDto = new ReadPediatricRecordDto
            {
                RecordId = recordId,
                WeightKg = 16.0m,
                HeightCm = 105.0m,
                HeartRate = 85
            };

            _serviceMock.Setup(s => s.UpdateAsync(recordId, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedDto);

            var controller = CreateController("Doctor");

            // Act
            var result = await controller.Update(recordId, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<ReadPediatricRecordDto>(okResult.Value);
            value.RecordId.Should().Be(recordId);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task Update_WithKeyNotFoundException_ReturnsNotFound()
        {
            // Arrange
            var recordId = 999;
            var dto = new UpdatePediatricRecordDto();
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
            var dto = new UpdatePediatricRecordDto();
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
        public async Task Delete_WithKeyNotFoundException_ReturnsNotFound()
        {
            // Arrange
            var recordId = 999;
            _serviceMock.Setup(s => s.DeleteAsync(recordId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Không tìm thấy hồ sơ"));

            var controller = CreateController("Nurse");

            // Act
            var result = await controller.Delete(recordId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
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
    }
}

