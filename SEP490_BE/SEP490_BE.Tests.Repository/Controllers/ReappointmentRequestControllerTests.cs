using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers.ReceptionistControllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers;

public class ReappointmentRequestControllerTests
{
    private readonly Mock<IReappointmentRequestService> _serviceMock = new(MockBehavior.Strict);
    private readonly ReappointmentRequestController _controller;

    public ReappointmentRequestControllerTests()
    {
        _controller = new ReappointmentRequestController(_serviceMock.Object);
        _controller.ControllerContext = ReceptionistContext();
    }

    private static ControllerContext ReceptionistContext()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "5"),
            new(ClaimTypes.Role, "Receptionist")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        return new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
    }

    // ===== GetPendingReappointmentRequests Tests =====

    [Fact]
    public async Task GetPendingReappointmentRequests_ReturnsOk_WithResults()
    {
        // Arrange
        var response = new PagedResponse<ReappointmentRequestDto>
        {
            Items = new List<ReappointmentRequestDto>
            {
                new ReappointmentRequestDto
                {
                    NotificationId = 1,
                    PatientName = "John Doe",
                    DoctorName = "Dr. Smith"
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };
        _serviceMock.Setup(s => s.GetPendingReappointmentRequestsAsync(
            5, 1, 10, null, "createdDate", "desc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetPendingReappointmentRequests(1, 10, null, "createdDate", "desc", default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        var value = okResult.Value as PagedResponse<ReappointmentRequestDto>;
        value.Should().NotBeNull();
        value!.Items.Should().HaveCount(1);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetPendingReappointmentRequests_ReturnsOk_WithSearchTerm()
    {
        // Arrange
        var response = new PagedResponse<ReappointmentRequestDto>
        {
            Items = new List<ReappointmentRequestDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };
        _serviceMock.Setup(s => s.GetPendingReappointmentRequestsAsync(
            5, 1, 10, "John", "createdDate", "desc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetPendingReappointmentRequests(1, 10, "John", "createdDate", "desc", default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetPendingReappointmentRequests_ReturnsBadRequest_WhenInvalidArguments()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetPendingReappointmentRequestsAsync(
            5, 1, 10, null, "createdDate", "desc", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid page number"));

        // Act
        var result = await _controller.GetPendingReappointmentRequests(1, 10, null, "createdDate", "desc", default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        badRequestResult.StatusCode.Should().Be(400);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetPendingReappointmentRequests_ReturnsUnauthorized_WhenUserIdNotFound()
    {
        // Arrange
        var controller = new ReappointmentRequestController(_serviceMock.Object);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, "Receptionist")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await controller.GetPendingReappointmentRequests(1, 10, null, "createdDate", "desc", default);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    // ===== GetReappointmentRequestById Tests =====

    [Fact]
    public async Task GetReappointmentRequestById_ReturnsOk_WhenFound()
    {
        // Arrange
        var request = new ReappointmentRequestDto
        {
            NotificationId = 1,
            PatientName = "John Doe",
            DoctorName = "Dr. Smith"
        };
        _serviceMock.Setup(s => s.GetReappointmentRequestByIdAsync(1, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Act
        var result = await _controller.GetReappointmentRequestById(1, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        var value = okResult.Value as ReappointmentRequestDto;
        value.Should().NotBeNull();
        value!.NotificationId.Should().Be(1);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetReappointmentRequestById_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetReappointmentRequestByIdAsync(999, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReappointmentRequestDto?)null);

        // Act
        var result = await _controller.GetReappointmentRequestById(999, default);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        notFoundResult.StatusCode.Should().Be(404);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetReappointmentRequestById_ReturnsBadRequest_WhenInvalidArguments()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetReappointmentRequestByIdAsync(1, 5, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid notification ID"));

        // Act
        var result = await _controller.GetReappointmentRequestById(1, default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        badRequestResult.StatusCode.Should().Be(400);
        _serviceMock.VerifyAll();
    }

    // ===== CompleteReappointmentRequest Tests =====

    [Fact]
    public async Task CompleteReappointmentRequest_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var request = new CompleteReappointmentRequestDto
        {
            NotificationId = 1,
            AppointmentDate = DateTime.Now.AddDays(7),
            ReasonForVisit = "Follow-up"
        };
        _serviceMock.Setup(s => s.CompleteReappointmentRequestAsync(request, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(100); // Returns appointment ID

        // Act
        var result = await _controller.CompleteReappointmentRequest(request, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        var value = okResult.Value;
        value.Should().NotBeNull();
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task CompleteReappointmentRequest_ReturnsBadRequest_WhenInvalidArguments()
    {
        // Arrange
        var request = new CompleteReappointmentRequestDto
        {
            NotificationId = 1,
            AppointmentDate = DateTime.Now.AddDays(-1), // Past date
            ReasonForVisit = "Follow-up"
        };
        _serviceMock.Setup(s => s.CompleteReappointmentRequestAsync(request, 5, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Appointment date cannot be in the past"));

        // Act
        var result = await _controller.CompleteReappointmentRequest(request, default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        badRequestResult.StatusCode.Should().Be(400);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task CompleteReappointmentRequest_ReturnsForbid_WhenUnauthorized()
    {
        // Arrange
        var request = new CompleteReappointmentRequestDto
        {
            NotificationId = 1,
            AppointmentDate = DateTime.Now.AddDays(7),
            ReasonForVisit = "Follow-up"
        };
        _serviceMock.Setup(s => s.CompleteReappointmentRequestAsync(request, 5, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("You are not authorized to complete this request"));

        // Act
        var result = await _controller.CompleteReappointmentRequest(request, default);

        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task CompleteReappointmentRequest_ReturnsUnauthorized_WhenUserIdNotFound()
    {
        // Arrange
        var controller = new ReappointmentRequestController(_serviceMock.Object);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, "Receptionist")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
        var request = new CompleteReappointmentRequestDto
        {
            NotificationId = 1,
            AppointmentDate = DateTime.Now.AddDays(7),
            ReasonForVisit = "Follow-up"
        };

        // Act
        var result = await controller.CompleteReappointmentRequest(request, default);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        unauthorizedResult.StatusCode.Should().Be(401);
    }
}

