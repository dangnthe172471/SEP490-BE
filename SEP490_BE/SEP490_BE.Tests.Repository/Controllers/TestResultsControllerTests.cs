using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.TestReDTO;
using SEP490_BE.DAL.Helpers;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers
{
    public class TestResultsControllerTests
    {
        private readonly Mock<ITestResultService> _serviceMock = new();

        private TestResultsController MakeController(string? role = "Nurse")
        {
            var controller = new TestResultsController(_serviceMock.Object);
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
        public async Task GetWorklist_WithValidParams_ReturnsOk()
        {
            // Arrange
            var worklist = new PagedResult<TestWorklistItemDto>
            {
                Items = new List<TestWorklistItemDto>
                {
                    new() { RecordId = 1, PatientName = "John Doe" }
                },
                PageNumber = 1,
                PageSize = 20,
                TotalCount = 1
            };
            _serviceMock.Setup(s => s.GetWorklistAsync(It.IsAny<TestWorklistQueryDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(worklist);

            var controller = MakeController("Nurse");

            // Act
            var result = await controller.GetWorklist(null, null, "All", 1, 20);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetWorklist_WithDateFilter_ReturnsOk()
        {
            // Arrange
            var date = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
            var worklist = new PagedResult<TestWorklistItemDto>
            {
                Items = new List<TestWorklistItemDto>(),
                PageNumber = 1,
                PageSize = 20,
                TotalCount = 0
            };
            _serviceMock.Setup(s => s.GetWorklistAsync(It.IsAny<TestWorklistQueryDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(worklist);

            var controller = MakeController("Nurse");

            // Act
            var result = await controller.GetWorklist(date, null, "Missing", 1, 20);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetWorklist_WithException_ReturnsInternalServerError()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetWorklistAsync(It.IsAny<TestWorklistQueryDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var controller = MakeController("Nurse");

            // Act
            var result = await controller.GetWorklist(null, null, "All", 1, 20);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetByRecordId_WithValidId_ReturnsOk()
        {
            // Arrange
            var results = new List<ReadTestResultDto>
            {
                new() { TestResultId = 1, RecordId = 1, TestName = "Blood Test" }
            };
            _serviceMock.Setup(s => s.GetByRecordIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(results);

            var controller = MakeController("Doctor");

            // Act
            var result = await controller.GetByRecordId(1);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByRecordId_WithException_ReturnsInternalServerError()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetByRecordIdAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var controller = MakeController("Doctor");

            // Act
            var result = await controller.GetByRecordId(1);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsOk()
        {
            // Arrange
            var testResult = new ReadTestResultDto
            {
                TestResultId = 1,
                RecordId = 1,
                TestName = "Blood Test",
                ResultValue = "Normal"
            };
            _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testResult);

            var controller = MakeController("Doctor");

            // Act
            var result = await controller.GetById(1);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_WithNotFound_ReturnsNotFound()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReadTestResultDto?)null);

            var controller = MakeController("Doctor");

            // Act
            var result = await controller.GetById(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Create_WithValidDto_ReturnsCreated()
        {
            // Arrange
            var dto = new CreateTestResultDto
            {
                RecordId = 1,
                TestTypeId = 1,
                ResultValue = "Normal"
            };
            var created = new ReadTestResultDto
            {
                TestResultId = 1,
                RecordId = 1,
                TestName = "Blood Test",
                ResultValue = "Normal"
            };
            _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            var controller = MakeController("Doctor");

            // Act
            var result = await controller.Create(dto);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task Create_WithArgumentNullException_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateTestResultDto { RecordId = 1 };
            _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentNullException("RecordId"));

            var controller = MakeController("Doctor");

            // Act
            var result = await controller.Create(dto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Create_WithKeyNotFoundException_ReturnsNotFound()
        {
            // Arrange
            var dto = new CreateTestResultDto { RecordId = 999 };
            _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Record not found"));

            var controller = MakeController("Doctor");

            // Act
            var result = await controller.Create(dto);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Update_WithValidDto_ReturnsOk()
        {
            // Arrange
            var dto = new UpdateTestResultDto { ResultValue = "Updated" };
            var updated = new ReadTestResultDto
            {
                TestResultId = 1,
                ResultValue = "Updated"
            };
            _serviceMock.Setup(s => s.UpdateAsync(1, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updated);

            var controller = MakeController("Doctor");

            // Act
            var result = await controller.Update(1, dto);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_WithKeyNotFoundException_ReturnsNotFound()
        {
            // Arrange
            var dto = new UpdateTestResultDto { ResultValue = "Updated" };
            _serviceMock.Setup(s => s.UpdateAsync(999, dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Test result not found"));

            var controller = MakeController("Doctor");

            // Act
            var result = await controller.Update(999, dto);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Delete_WithValidId_ReturnsNoContent()
        {
            // Arrange
            _serviceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var controller = MakeController("Doctor");

            // Act
            var result = await controller.Delete(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_WithKeyNotFoundException_ReturnsNotFound()
        {
            // Arrange
            _serviceMock.Setup(s => s.DeleteAsync(999, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Test result not found"));

            var controller = MakeController("Doctor");

            // Act
            var result = await controller.Delete(999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetTypes_ReturnsOk()
        {
            // Arrange
            var types = new List<TestTypeLite>
            {
                new() { TestTypeId = 1, TestName = "Blood Test" }
            };
            _serviceMock.Setup(s => s.GetTestTypesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(types);

            var controller = MakeController("Doctor");

            // Act
            var result = await controller.GetTypes();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetTypes_WithException_ReturnsInternalServerError()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetTestTypesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var controller = MakeController("Doctor");

            // Act
            var result = await controller.GetTypes();

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }
    }
}




