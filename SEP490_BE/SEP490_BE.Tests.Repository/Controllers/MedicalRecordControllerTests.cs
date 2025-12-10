using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.MedicalRecordDTO;
using SEP490_BE.DAL.Models;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers;

public class MedicalRecordControllerTests
{
    private readonly Mock<IMedicalRecordService> _serviceMock = new(MockBehavior.Strict);
    private readonly MedicalRecordController _controller;

    public MedicalRecordControllerTests()
    {
        _controller = new MedicalRecordController(_serviceMock.Object);
    }

    private static ControllerContext DoctorContext()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Role, "Doctor")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        return new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
    }

    private static MedicalRecord CreateCompleteMedicalRecord(int recordId, int appointmentId, string? diagnosis = "Test Diagnosis", string? doctorNotes = "Test Notes")
    {
        return new MedicalRecord
        {
            RecordId = recordId,
            AppointmentId = appointmentId,
            Diagnosis = diagnosis,
            DoctorNotes = doctorNotes,
            CreatedAt = DateTime.Now
        };
    }

    // ===== GetAllAsync Tests =====

    [Fact]
    public async Task GetAllAsync_UTCID01_ReturnsOk_WithRecords()
    {
        // Arrange
        var records = new List<MedicalRecord>
        {
            CreateCompleteMedicalRecord(1, 1, "Diagnosis 1", "Notes 1"),
            CreateCompleteMedicalRecord(2, 2, "Diagnosis 2", "Notes 2")
        };
        _serviceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        // Act
        var result = await _controller.GetAllAsync(default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        var resultList = okResult.Value as List<MedicalRecord>;
        resultList.Should().HaveCount(2);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetAllAsync_UTCID02_ReturnsOk_WithEmptyList()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MedicalRecord>());

        // Act
        var result = await _controller.GetAllAsync(default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        var resultList = okResult.Value as List<MedicalRecord>;
        resultList.Should().BeEmpty();
        _serviceMock.VerifyAll();
    }

    // ===== GetByIdAsync Tests =====

    [Fact]
    public async Task GetByIdAsync_UTCID01_ReturnsOk_WhenRecordExists()
    {
        // Arrange
        var record = CreateCompleteMedicalRecord(1, 1, "Test Diagnosis", "Test Notes");
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        // Act
        var result = await _controller.GetByIdAsync(1, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        var resultRecord = okResult.Value as MedicalRecord;
        resultRecord.Should().NotBeNull();
        resultRecord!.RecordId.Should().Be(1);
        resultRecord.Diagnosis.Should().Be("Test Diagnosis");
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetByIdAsync_UTCID02_ReturnsNotFound_WhenRecordNotExists()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalRecord?)null);

        // Act
        var result = await _controller.GetByIdAsync(999, default);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        notFoundResult.StatusCode.Should().Be(404);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetByIdAsync_UTCID03_ReturnsNotFound_WhenIdIsZero()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(0, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalRecord?)null);

        // Act
        var result = await _controller.GetByIdAsync(0, default);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        notFoundResult.StatusCode.Should().Be(404);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetByIdAsync_UTCID04_ReturnsOk_WhenRecordHasNullFields()
    {
        // Arrange
        var record = new MedicalRecord
        {
            RecordId = 1,
            AppointmentId = 1,
            Diagnosis = null,
            DoctorNotes = null,
            CreatedAt = DateTime.Now
        };
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        // Act
        var result = await _controller.GetByIdAsync(1, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        var resultRecord = okResult.Value as MedicalRecord;
        resultRecord.Should().NotBeNull();
        resultRecord!.Diagnosis.Should().BeNull();
        resultRecord.DoctorNotes.Should().BeNull();
        _serviceMock.VerifyAll();
    }

    // ===== CreateAsync Tests =====

    [Fact]
    public async Task CreateAsync_UTCID01_ReturnsCreated_WhenValidRequest()
    {
        // Arrange
        _controller.ControllerContext = DoctorContext();
        var dto = new CreateMedicalRecordDto
        {
            AppointmentId = 1,
            Diagnosis = "Test Diagnosis",
            DoctorNotes = "Test Notes"
        };
        var created = CreateCompleteMedicalRecord(10, 1, "Test Diagnosis", "Test Notes");
        _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.CreateAsync(dto, default);

        // Assert
        var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
        createdResult.StatusCode.Should().Be(201);
        createdResult.RouteName.Should().Be("GetMedicalRecordById");
        createdResult.RouteValues!["id"].Should().Be(10);
        var resultRecord = createdResult.Value as MedicalRecord;
        resultRecord.Should().NotBeNull();
        resultRecord!.Diagnosis.Should().Be("Test Diagnosis");
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task CreateAsync_UTCID02_ReturnsCreated_WhenOnlyAppointmentIdProvided()
    {
        // Arrange
        _controller.ControllerContext = DoctorContext();
        var dto = new CreateMedicalRecordDto
        {
            AppointmentId = 1,
            Diagnosis = null,
            DoctorNotes = null
        };
        var created = CreateCompleteMedicalRecord(10, 1, null, null);
        _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.CreateAsync(dto, default);

        // Assert
        var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
        createdResult.StatusCode.Should().Be(201);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task CreateAsync_UTCID03_ReturnsCreated_WhenOnlyDiagnosisProvided()
    {
        // Arrange
        _controller.ControllerContext = DoctorContext();
        var dto = new CreateMedicalRecordDto
        {
            AppointmentId = 1,
            Diagnosis = "Test Diagnosis",
            DoctorNotes = null
        };
        var created = CreateCompleteMedicalRecord(10, 1, "Test Diagnosis", null);
        _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.CreateAsync(dto, default);

        // Assert
        var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
        createdResult.StatusCode.Should().Be(201);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task CreateAsync_UTCID04_ReturnsCreated_WhenOnlyDoctorNotesProvided()
    {
        // Arrange
        _controller.ControllerContext = DoctorContext();
        var dto = new CreateMedicalRecordDto
        {
            AppointmentId = 1,
            Diagnosis = null,
            DoctorNotes = "Test Notes"
        };
        var created = CreateCompleteMedicalRecord(10, 1, null, "Test Notes");
        _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.CreateAsync(dto, default);

        // Assert
        var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
        createdResult.StatusCode.Should().Be(201);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task CreateAsync_UTCID05_ReturnsCreated_WhenAppointmentIdIsZero()
    {
        // Arrange
        _controller.ControllerContext = DoctorContext();
        var dto = new CreateMedicalRecordDto
        {
            AppointmentId = 0,
            Diagnosis = "Test Diagnosis",
            DoctorNotes = "Test Notes"
        };
        var created = CreateCompleteMedicalRecord(10, 0, "Test Diagnosis", "Test Notes");
        _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.CreateAsync(dto, default);

        // Assert
        var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
        createdResult.StatusCode.Should().Be(201);
        _serviceMock.VerifyAll();
    }

    // ===== UpdateAsync Tests =====

    [Fact]
    public async Task UpdateAsync_UTCID01_ReturnsOk_WhenValidRequest()
    {
        // Arrange
        _controller.ControllerContext = DoctorContext();
        var dto = new UpdateMedicalRecordDto
        {
            Diagnosis = "Updated Diagnosis",
            DoctorNotes = "Updated Notes"
        };
        var updated = CreateCompleteMedicalRecord(5, 1, "Updated Diagnosis", "Updated Notes");
        _serviceMock.Setup(s => s.UpdateAsync(5, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _controller.UpdateAsync(5, dto, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        var resultRecord = okResult.Value as MedicalRecord;
        resultRecord.Should().NotBeNull();
        resultRecord!.Diagnosis.Should().Be("Updated Diagnosis");
        resultRecord.DoctorNotes.Should().Be("Updated Notes");
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task UpdateAsync_UTCID02_ReturnsOk_WhenOnlyDiagnosisUpdated()
    {
        // Arrange
        _controller.ControllerContext = DoctorContext();
        var dto = new UpdateMedicalRecordDto
        {
            Diagnosis = "Updated Diagnosis",
            DoctorNotes = null
        };
        var updated = CreateCompleteMedicalRecord(5, 1, "Updated Diagnosis", null);
        _serviceMock.Setup(s => s.UpdateAsync(5, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _controller.UpdateAsync(5, dto, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task UpdateAsync_UTCID03_ReturnsOk_WhenOnlyDoctorNotesUpdated()
    {
        // Arrange
        _controller.ControllerContext = DoctorContext();
        var dto = new UpdateMedicalRecordDto
        {
            Diagnosis = null,
            DoctorNotes = "Updated Notes"
        };
        var updated = CreateCompleteMedicalRecord(5, 1, null, "Updated Notes");
        _serviceMock.Setup(s => s.UpdateAsync(5, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _controller.UpdateAsync(5, dto, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task UpdateAsync_UTCID04_ReturnsNotFound_WhenRecordNotExists()
    {
        // Arrange
        _controller.ControllerContext = DoctorContext();
        var dto = new UpdateMedicalRecordDto { Diagnosis = "Updated Diagnosis" };
        _serviceMock.Setup(s => s.UpdateAsync(999, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalRecord?)null);

        // Act
        var result = await _controller.UpdateAsync(999, dto, default);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        notFoundResult.StatusCode.Should().Be(404);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task UpdateAsync_UTCID05_ReturnsNotFound_WhenIdIsZero()
    {
        // Arrange
        _controller.ControllerContext = DoctorContext();
        var dto = new UpdateMedicalRecordDto { Diagnosis = "Updated Diagnosis" };
        _serviceMock.Setup(s => s.UpdateAsync(0, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalRecord?)null);

        // Act
        var result = await _controller.UpdateAsync(0, dto, default);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        notFoundResult.StatusCode.Should().Be(404);
        _serviceMock.VerifyAll();
    }

    // ===== GetByAppointmentIdAsync Tests =====

    [Fact]
    public async Task GetByAppointmentIdAsync_UTCID01_ReturnsOk_WhenRecordExists()
    {
        // Arrange
        var record = CreateCompleteMedicalRecord(1, 1, "Test Diagnosis", "Test Notes");
        _serviceMock.Setup(s => s.GetByAppointmentIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        // Act
        var result = await _controller.GetByAppointmentIdAsync(1, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        var resultRecord = okResult.Value as MedicalRecord;
        resultRecord.Should().NotBeNull();
        resultRecord!.AppointmentId.Should().Be(1);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetByAppointmentIdAsync_UTCID02_ReturnsNotFound_WhenRecordNotExists()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByAppointmentIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalRecord?)null);

        // Act
        var result = await _controller.GetByAppointmentIdAsync(999, default);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        notFoundResult.StatusCode.Should().Be(404);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetByAppointmentIdAsync_UTCID03_ReturnsNotFound_WhenAppointmentIdIsZero()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByAppointmentIdAsync(0, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalRecord?)null);

        // Act
        var result = await _controller.GetByAppointmentIdAsync(0, default);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        notFoundResult.StatusCode.Should().Be(404);
        _serviceMock.VerifyAll();
    }

    // ===== GetAllByDoctorAsync Tests =====

    [Fact]
    public async Task GetAllByDoctorAsync_UTCID01_ReturnsOk_WithRecords()
    {
        // Arrange
        var records = new List<MedicalRecord>
        {
            CreateCompleteMedicalRecord(1, 1, "Diagnosis 1", "Notes 1"),
            CreateCompleteMedicalRecord(2, 2, "Diagnosis 2", "Notes 2")
        };
        _serviceMock.Setup(s => s.GetAllByDoctorAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        // Act
        var result = await _controller.GetAllByDoctorAsync(1, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        var resultList = okResult.Value as List<MedicalRecord>;
        resultList.Should().HaveCount(2);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetAllByDoctorAsync_UTCID02_ReturnsOk_WithEmptyList()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetAllByDoctorAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MedicalRecord>());

        // Act
        var result = await _controller.GetAllByDoctorAsync(1, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        var resultMessage = okResult.Value as string;
        resultMessage.Should().Be("Không có hồ sơ bệnh án");
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetAllByDoctorAsync_UTCID03_ReturnsOk_WhenNullReturned()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetAllByDoctorAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<MedicalRecord>?)null);

        // Act
        var result = await _controller.GetAllByDoctorAsync(1, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        var resultMessage = okResult.Value as string;
        resultMessage.Should().Be("Không có hồ sơ bệnh án");
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetAllByDoctorAsync_UTCID04_ReturnsOk_WhenDoctorIdIsZero()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetAllByDoctorAsync(0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MedicalRecord>());

        // Act
        var result = await _controller.GetAllByDoctorAsync(0, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        var resultMessage = okResult.Value as string;
        resultMessage.Should().Be("Không có hồ sơ bệnh án");
        _serviceMock.VerifyAll();
    }
}
