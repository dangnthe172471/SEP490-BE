using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SEP490_BE.Tests.Controllers;

public class ManagerShiftSwapControllerTests
{
    private readonly Mock<IDoctorShiftExchangeService> _service = new(MockBehavior.Strict);
    private ManagerShiftSwapController NewController() => new(_service.Object);

    #region Helper Methods

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
            Assert.Contains(expectedMessage, messageValue!);
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
            Assert.Contains(expectedMessage, messageValue!);
        }
    }

    private static void AssertInternalServerErrorResult(IActionResult result, string? expectedMessage = null)
    {
        var statusCode = Assert.IsType<ObjectResult>(result);
        statusCode.StatusCode.Should().Be(500);
        statusCode.Value.Should().NotBeNull();

        var valueType = statusCode.Value!.GetType();
        var successProperty = valueType.GetProperty("success");
        var successValue = successProperty?.GetValue(statusCode.Value);
        Assert.False((bool)successValue!);

        if (expectedMessage != null)
        {
            var messageProperty = valueType.GetProperty("message");
            var messageValue = messageProperty?.GetValue(statusCode.Value)?.ToString();
            Assert.Contains(expectedMessage, messageValue!);
        }
    }

    #endregion

    #region ReviewShiftSwapRequest Tests

    [Fact]
    public async Task ReviewShiftSwapRequest_ApproveRequest_ReturnsOk()
    {
        // Arrange - Test Case UTCID01: Approve request successfully
        var review = new ReviewShiftSwapRequestDTO
        {
            ExchangeId = 1,
            Status = "Approved"
        };

        _service.Setup(s => s.ReviewShiftSwapRequestAsync(review))
            .ReturnsAsync(true);

        // Act
        var result = await NewController().ReviewShiftSwapRequest(review);

        // Assert
        AssertOkResult(result, expectedMessage: "chấp nhận yêu cầu đổi ca thành công");
        _service.VerifyAll();
    }

    [Fact]
    public async Task ReviewShiftSwapRequest_RejectRequest_ReturnsOk()
    {
        // Arrange - Test Case UTCID02: Reject request successfully
        var review = new ReviewShiftSwapRequestDTO
        {
            ExchangeId = 1,
            Status = "Rejected"
        };

        _service.Setup(s => s.ReviewShiftSwapRequestAsync(review))
            .ReturnsAsync(true);

        // Act
        var result = await NewController().ReviewShiftSwapRequest(review);

        // Assert
        AssertOkResult(result, expectedMessage: "từ chối yêu cầu đổi ca thành công");
        _service.VerifyAll();
    }

    [Fact]
    public async Task ReviewShiftSwapRequest_ServiceReturnsFalse_ReturnsBadRequest()
    {
        // Arrange - Test Case UTCID03: Service returns false
        var review = new ReviewShiftSwapRequestDTO
        {
            ExchangeId = 1,
            Status = "Approved"
        };

        _service.Setup(s => s.ReviewShiftSwapRequestAsync(review))
            .ReturnsAsync(false);

        // Act
        var result = await NewController().ReviewShiftSwapRequest(review);

        // Assert
        AssertBadRequestResult(result, expectedMessage: "Không thể cập nhật trạng thái yêu cầu");
        _service.VerifyAll();
    }

    [Fact]
    public async Task ReviewShiftSwapRequest_ArgumentException_ReturnsBadRequest()
    {
        // Arrange - Test Case UTCID04: Invalid request (ArgumentException)
        var review = new ReviewShiftSwapRequestDTO
        {
            ExchangeId = 999,
            Status = "Invalid"
        };

        _service.Setup(s => s.ReviewShiftSwapRequestAsync(review))
            .ThrowsAsync(new ArgumentException("Invalid exchange ID"));

        // Act
        var result = await NewController().ReviewShiftSwapRequest(review);

        // Assert
        AssertBadRequestResult(result, expectedMessage: "Invalid exchange ID");
        _service.VerifyAll();
    }

    [Fact]
    public async Task ReviewShiftSwapRequest_InvalidOperationException_ReturnsBadRequest()
    {
        // Arrange - Test Case UTCID05: Invalid operation (InvalidOperationException)
        var review = new ReviewShiftSwapRequestDTO
        {
            ExchangeId = 1,
            Status = "Approved"
        };

        _service.Setup(s => s.ReviewShiftSwapRequestAsync(review))
            .ThrowsAsync(new InvalidOperationException("Request already processed"));

        // Act
        var result = await NewController().ReviewShiftSwapRequest(review);

        // Assert
        AssertBadRequestResult(result, expectedMessage: "Request already processed");
        _service.VerifyAll();
    }

    [Fact]
    public async Task ReviewShiftSwapRequest_GeneralException_ReturnsInternalServerError()
    {
        // Arrange - Test Case UTCID06: General exception
        var review = new ReviewShiftSwapRequestDTO
        {
            ExchangeId = 1,
            Status = "Approved"
        };

        _service.Setup(s => s.ReviewShiftSwapRequestAsync(review))
            .ThrowsAsync(new Exception("Database connection error"));

        // Act
        var result = await NewController().ReviewShiftSwapRequest(review);

        // Assert
        AssertInternalServerErrorResult(result, expectedMessage: "Lỗi hệ thống");
        _service.VerifyAll();
    }

    #endregion

    #region GetAllRequests Tests

    [Fact]
    public async Task GetAllRequests_ReturnsOk_WithRequests()
    {
        // Arrange - Test Case UTCID07: Get all requests successfully
        var expectedRequests = new List<ShiftSwapRequestResponseDTO>
        {
            new()
            {
                ExchangeId = 1,
                Doctor1Id = 1,
                Doctor1Name = "Bác sĩ A",
                Doctor2Id = 2,
                Doctor2Name = "Bác sĩ B",
                Status = "Pending"
            },
            new()
            {
                ExchangeId = 2,
                Doctor1Id = 3,
                Doctor1Name = "Bác sĩ C",
                Doctor2Id = 4,
                Doctor2Name = "Bác sĩ D",
                Status = "Approved"
            }
        };

        _service.Setup(s => s.GetAllRequestsAsync())
            .ReturnsAsync(expectedRequests);

        // Act
        var result = await NewController().GetAllRequests();

        // Assert
        AssertOkResult(result);
        _service.VerifyAll();
    }

    [Fact]
    public async Task GetAllRequests_ExceptionThrown_ReturnsBadRequest()
    {
        // Arrange - Test Case UTCID08: Exception thrown
        _service.Setup(s => s.GetAllRequestsAsync())
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await NewController().GetAllRequests();

        // Assert
        AssertBadRequestResult(result, expectedMessage: "Service error");
        _service.VerifyAll();
    }

    #endregion

    #region GetRequestDetails Tests

    [Fact]
    public async Task GetRequestDetails_ReturnsOk_WithRequestDetails()
    {
        // Arrange - Test Case UTCID09: Get request details successfully
        var exchangeId = 1;
        var expectedRequest = new ShiftSwapRequestResponseDTO
        {
            ExchangeId = exchangeId,
            Doctor1Id = 1,
            Doctor1Name = "Bác sĩ A",
            Doctor2Id = 2,
            Doctor2Name = "Bác sĩ B",
            Status = "Pending",
            SwapType = "Temporary"
        };

        _service.Setup(s => s.GetRequestByIdAsync(exchangeId))
            .ReturnsAsync(expectedRequest);

        // Act
        var result = await NewController().GetRequestDetails(exchangeId);

        // Assert
        AssertOkResult(result);
        _service.VerifyAll();
    }

    [Fact]
    public async Task GetRequestDetails_RequestNotFound_ReturnsNotFound()
    {
        // Arrange - Test Case UTCID10: Request not found
        var exchangeId = 999;

        _service.Setup(s => s.GetRequestByIdAsync(exchangeId))
            .ReturnsAsync((ShiftSwapRequestResponseDTO?)null);

        // Act
        var result = await NewController().GetRequestDetails(exchangeId);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        notFound.StatusCode.Should().Be(404);
        notFound.Value.Should().NotBeNull();

        var valueType = notFound.Value!.GetType();
        var successProperty = valueType.GetProperty("success");
        var successValue = successProperty?.GetValue(notFound.Value);
        Assert.False((bool)successValue!);

        var messageProperty = valueType.GetProperty("message");
        var messageValue = messageProperty?.GetValue(notFound.Value)?.ToString();
        Assert.Contains("Không tìm thấy yêu cầu đổi ca", messageValue!);

        _service.VerifyAll();
    }

    [Fact]
    public async Task GetRequestDetails_ExceptionThrown_ReturnsBadRequest()
    {
        // Arrange - Test Case UTCID11: Exception thrown
        var exchangeId = 1;

        _service.Setup(s => s.GetRequestByIdAsync(exchangeId))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await NewController().GetRequestDetails(exchangeId);

        // Assert
        AssertBadRequestResult(result, expectedMessage: "Service error");
        _service.VerifyAll();
    }

    #endregion
}

