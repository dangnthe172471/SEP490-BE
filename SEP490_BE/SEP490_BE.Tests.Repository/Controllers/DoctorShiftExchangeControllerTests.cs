using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_BE.Tests.Controllers;

public class DoctorShiftExchangeControllerTests
{
    private readonly Mock<IDoctorShiftExchangeService> _service = new(MockBehavior.Strict);
    private DoctorShiftExchangeController NewController() => new(_service.Object);

    #region Helper Methods

    private static CreateShiftSwapRequestDTO CreateValidRequest() => new()
    {
        Doctor1Id = 1,
        Doctor2Id = 2,
        Doctor1ShiftRefId = 10,
        Doctor2ShiftRefId = 20,
        ExchangeDate = new DateOnly(2025, 12, 1),
        SwapType = "Temporary"
    };

    private static CreateShiftSwapRequestDTO CreateInvalidRequest() => new()
    {
        Doctor1Id = 1,
        Doctor2Id = 1, // Same doctor - invalid
        Doctor1ShiftRefId = 10,
        Doctor2ShiftRefId = 20,
        ExchangeDate = new DateOnly(2025, 12, 1),
        SwapType = "Temporary"
    };

    private static void AssertOkResult(IActionResult result, bool expectSuccess = true, string? expectedMessage = null)
    {
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().NotBeNull();

        var valueType = okResult.Value!.GetType();
        var successProperty = valueType.GetProperty("success");
        var successValue = successProperty?.GetValue(okResult.Value);
        Assert.Equal(expectSuccess, (bool)successValue!);

        if (expectedMessage != null)
        {
            var messageProperty = valueType.GetProperty("message");
            var messageValue = messageProperty?.GetValue(okResult.Value)?.ToString();
            Assert.Equal(expectedMessage, messageValue);
        }
    }

    private static void AssertBadRequestResult(IActionResult result, string? expectedMessage = null)
    {
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        badRequest.StatusCode.Should().Be(400);
        badRequest.Value.Should().NotBeNull();

        var valueType = badRequest.Value!.GetType();
        var successProperty = valueType.GetProperty("success");
        var successValue = successProperty?.GetValue(badRequest.Value);
        Assert.False((bool)successValue!);

        if (expectedMessage != null)
        {
            var messageProperty = valueType.GetProperty("message");
            var messageValue = messageProperty?.GetValue(badRequest.Value)?.ToString();
            Assert.Equal(expectedMessage, messageValue);
        }
    }

    #endregion

    #region CreateShiftSwapRequest Tests

    [Fact]
    public async Task CreateShiftSwapRequest_ReturnsOk_WhenValid()
    {
        // Arrange
        var request = CreateValidRequest();
        var expectedResponse = new ShiftSwapRequestResponseDTO
        {
            ExchangeId = 1,
            Doctor1Id = 1,
            Doctor1Name = "Bác sĩ A",
            Doctor2Id = 2,
            Doctor2Name = "Bác sĩ B",
            Status = "Pending",
            SwapType = "Temporary"
        };

        _service.Setup(s => s.CreateShiftSwapRequestAsync(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await NewController().CreateShiftSwapRequest(request);

        // Assert
        AssertOkResult(result, expectedMessage: "Yêu cầu đổi ca đã được tạo thành công");
        _service.VerifyAll();
    }

    [Fact]
    public async Task CreateShiftSwapRequest_ReturnsBadRequest_WhenInvalid()
    {
        // Arrange
        var request = CreateInvalidRequest();
        _service.Setup(s => s.CreateShiftSwapRequestAsync(request))
            .ThrowsAsync(new ArgumentException("Invalid shift swap request"));

        // Act
        var result = await NewController().CreateShiftSwapRequest(request);

        // Assert
        AssertBadRequestResult(result);
        _service.VerifyAll();
    }

    #endregion

    #region GetRequestsByDoctorId Tests

    [Fact]
    public async Task GetRequestsByDoctorId_ReturnsOk_WithRequests()
    {
        // Arrange
        var doctorId = 1;
        var expectedRequests = new List<ShiftSwapRequestResponseDTO>
        {
            new() { ExchangeId = 1, Doctor1Id = doctorId, Doctor1Name = "Bác sĩ A", Status = "Pending" },
            new() { ExchangeId = 2, Doctor1Id = doctorId, Doctor1Name = "Bác sĩ A", Status = "Approved" }
        };

        _service.Setup(s => s.GetRequestsByDoctorIdAsync(doctorId))
            .ReturnsAsync(expectedRequests);

        // Act
        var result = await NewController().GetRequestsByDoctorId(doctorId);

        // Assert
        AssertOkResult(result);
        _service.VerifyAll();
    }

    [Fact]
    public async Task GetRequestsByDoctorId_ReturnsOk_WithEmptyList()
    {
        // Arrange
        var doctorId = 999;
        _service.Setup(s => s.GetRequestsByDoctorIdAsync(doctorId))
            .ReturnsAsync(new List<ShiftSwapRequestResponseDTO>());

        // Act
        var result = await NewController().GetRequestsByDoctorId(doctorId);

        // Assert
        AssertOkResult(result);
        _service.VerifyAll();
    }

    #endregion

    #region GetDoctorShifts Tests

    [Fact]
    public async Task GetDoctorShifts_ReturnsOk_WithShifts()
    {
        // Arrange
        var doctorId = 1;
        var from = new DateOnly(2025, 1, 1);
        var to = new DateOnly(2025, 12, 31);
        var expectedShifts = new List<DoctorShiftDTO>
        {
            new()
            {
                DoctorShiftId = 1,
                DoctorId = doctorId,
                DoctorName = "Bác sĩ A",
                ShiftType = "Sáng",
                EffectiveFrom = from,
                EffectiveTo = to
            }
        };

        _service.Setup(s => s.GetDoctorShiftsAsync(doctorId, from, to))
            .ReturnsAsync(expectedShifts);

        // Act
        var result = await NewController().GetDoctorShifts(doctorId, from, to);

        // Assert
        AssertOkResult(result);
        _service.VerifyAll();
    }

    [Fact]
    public async Task GetDoctorShifts_ReturnsOk_WithEmptyList()
    {
        // Arrange
        var doctorId = 999;
        var from = new DateOnly(2025, 1, 1);
        var to = new DateOnly(2025, 12, 31);

        _service.Setup(s => s.GetDoctorShiftsAsync(doctorId, from, to))
            .ReturnsAsync(new List<DoctorShiftDTO>());

        // Act
        var result = await NewController().GetDoctorShifts(doctorId, from, to);

        // Assert
        AssertOkResult(result);
        _service.VerifyAll();
    }

    #endregion

    #region GetDoctorsBySpecialty Tests

    [Fact]
    public async Task GetDoctorsBySpecialty_ReturnsOk_WithDoctors()
    {
        // Arrange
        var specialty = "Nội khoa";
        var expectedDoctors = new List<DoctorDTO>
        {
            new() { DoctorID = 1, FullName = "Bác sĩ A", Specialty = specialty, Email = "doctor1@example.com" },
            new() { DoctorID = 2, FullName = "Bác sĩ B", Specialty = specialty, Email = "doctor2@example.com" }
        };

        _service.Setup(s => s.GetDoctorsBySpecialtyAsync(specialty))
            .ReturnsAsync(expectedDoctors);

        // Act
        var result = await NewController().GetDoctorsBySpecialty(specialty);

        // Assert
        AssertOkResult(result);
        _service.VerifyAll();
    }

    [Fact]
    public async Task GetDoctorsBySpecialty_ReturnsOk_WithEmptyList()
    {
        // Arrange
        var specialty = "Không tồn tại";
        _service.Setup(s => s.GetDoctorsBySpecialtyAsync(specialty))
            .ReturnsAsync(new List<DoctorDTO>());

        // Act
        var result = await NewController().GetDoctorsBySpecialty(specialty);

        // Assert
        AssertOkResult(result);
        _service.VerifyAll();
    }

    #endregion
}
