using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.DermatologyDTO;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers
{
    public class DermatologyRecordsControllerTests
    {
        private readonly Mock<IDermatologyRecordService> _serviceMock = new(MockBehavior.Strict);

        private DermatologyRecordsController CreateController(string? role = "Doctor")
        {
            var controller = new DermatologyRecordsController(_serviceMock.Object);
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
            var expectedDto = new ReadDermatologyRecordDto
            {
                RecordId = recordId,
                RequestedProcedure = "Khám da liễu",
                BodyArea = "Face",
                ProcedureNotes = "No lesions"
            };

            _serviceMock.Setup(s => s.GetByRecordIdAsync(recordId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var controller = CreateController("Doctor");

            // Act
            var result = await controller.Get(recordId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<ReadDermatologyRecordDto>(okResult.Value);
            value.RecordId.Should().Be(recordId);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task Get_WithNonExistentRecordId_ReturnsNotFound()
        {
            // Arrange
            var recordId = 999;
            _serviceMock.Setup(s => s.GetByRecordIdAsync(recordId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReadDermatologyRecordDto?)null);

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
            var dto = new CreateDermatologyRecordDto
            {
                RecordId = 1,
                RequestedProcedure = "Khám da liễu",
                BodyArea = "Face",
                ProcedureNotes = "No lesions"
            };

            var createdDto = new ReadDermatologyRecordDto
            {
                RecordId = 1,
                RequestedProcedure = "Khám da liễu",
                BodyArea = "Face",
                ProcedureNotes = "No lesions"
            };

            _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdDto);

            var controller = CreateController("Doctor");

            // Act
            var result = await controller.Create(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsType<ReadDermatologyRecordDto>(createdResult.Value);
            value.RecordId.Should().Be(1);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task Create_WithArgumentNullException_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateDermatologyRecordDto();
            _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentNullException("dto", "Dữ liệu không được để trống"));

            var controller = CreateController("Nurse");

            // Act
            var result = await controller.Create(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task Create_WithKeyNotFoundException_ReturnsNotFound()
        {
            // Arrange
            var dto = new CreateDermatologyRecordDto { RecordId = 999 };
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
            var dto = new CreateDermatologyRecordDto { RecordId = 1 };
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
            var dto = new CreateDermatologyRecordDto { RecordId = 1 };
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
            var dto = new UpdateDermatologyRecordDto
            {
                RequestedProcedure = "Khám da liễu",
                BodyArea = "Arms",
                ProcedureNotes = "Rash present"
            };

            var updatedDto = new ReadDermatologyRecordDto
            {
                RecordId = recordId,
                RequestedProcedure = "Khám da liễu",
                BodyArea = "Arms",
                ProcedureNotes = "Rash present"
            };

            _serviceMock.Setup(s => s.UpdateAsync(recordId, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedDto);

            var controller = CreateController("Doctor");

            // Act
            var result = await controller.Update(recordId, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<ReadDermatologyRecordDto>(okResult.Value);
            value.RecordId.Should().Be(recordId);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task Update_WithKeyNotFoundException_ReturnsNotFound()
        {
            // Arrange
            var recordId = 999;
            var dto = new UpdateDermatologyRecordDto();
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
            var dto = new UpdateDermatologyRecordDto();
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

