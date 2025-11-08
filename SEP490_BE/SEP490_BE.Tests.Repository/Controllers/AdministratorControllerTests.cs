using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers;

public class AdministratorControllerTests
{
    private readonly Mock<IAdministratorService> _serviceMock = new(MockBehavior.Strict);
    private readonly AdministratorController _controller;

    public AdministratorControllerTests()
    {
        _controller = new AdministratorController(_serviceMock.Object);
    }

    private static ControllerContext AdminContext()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Role, "Administrator")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        return new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
    }

    private static ControllerContext UserContext(int userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, "Patient")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        return new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
    }

    // ===== GetAll Tests =====

    [Fact]
    public async Task GetAll_ReturnsOk_WithUsers()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new UserDto { UserId = 1, FullName = "User 1", Phone = "0909123456" },
            new UserDto { UserId = 2, FullName = "User 2", Phone = "0909123457" }
        };
        _serviceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetAll(default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(users);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoUsers()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserDto>());

        // Act
        var result = await _controller.GetAll(default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        ((IEnumerable<UserDto>)okResult.Value!).Should().BeEmpty();
        _serviceMock.VerifyAll();
    }

    // ===== GetById Tests =====

    [Fact]
    public async Task GetById_ReturnsOk_WhenUserExists()
    {
        // Arrange
        var user = new UserDto { UserId = 1, FullName = "Test User", Phone = "0909123456" };
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetById(1, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(user);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenUserNotExists()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.GetById(999, default);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
        notFoundResult.StatusCode.Should().Be(404);
        _serviceMock.VerifyAll();
    }

    // ===== CreateUser Tests =====

    [Fact]
    public async Task CreateUser_ReturnsCreated_WhenValidRequest()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var request = new CreateUserRequest
        {
            Phone = "0909123456",
            Password = "123456",
            FullName = "New User",
            RoleId = 2
        };
        var createdUser = new UserDto { UserId = 100, FullName = request.FullName, Phone = request.Phone };
        _serviceMock.Setup(s => s.CreateUserAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateUser(request, default);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().BeEquivalentTo(createdUser);
        createdResult.ActionName.Should().Be(nameof(AdministratorController.GetById));
        createdResult.RouteValues!["id"].Should().Be(100);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task CreateUser_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        _controller.ModelState.AddModelError("Phone", "Phone is required");
        var request = new CreateUserRequest();

        // Act
        var result = await _controller.CreateUser(request, default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        badRequestResult.StatusCode.Should().Be(400);
        _serviceMock.Verify(s => s.CreateUserAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateUser_ReturnsBadRequest_WhenServiceReturnsNull()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var request = new CreateUserRequest
        {
            Phone = "0909123456",
            Password = "123456",
            FullName = "New User",
            RoleId = 2
        };
        _serviceMock.Setup(s => s.CreateUserAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.CreateUser(request, default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        badRequestResult.StatusCode.Should().Be(400);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task CreateUser_ReturnsBadRequest_WhenInvalidOperationException()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var request = new CreateUserRequest
        {
            Phone = "0909123456",
            Password = "123456",
            FullName = "New User",
            RoleId = 2
        };
        _serviceMock.Setup(s => s.CreateUserAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Số điện thoại đã được sử dụng."));

        // Act
        var result = await _controller.CreateUser(request, default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        badRequestResult.StatusCode.Should().Be(400);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task CreateUser_ReturnsInternalServerError_WhenException()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var request = new CreateUserRequest
        {
            Phone = "0909123456",
            Password = "123456",
            FullName = "New User",
            RoleId = 2
        };
        _serviceMock.Setup(s => s.CreateUserAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CreateUser(request, default);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        statusCodeResult.StatusCode.Should().Be(500);
        _serviceMock.VerifyAll();
    }

    // ===== UpdateUser Tests =====

    [Fact]
    public async Task UpdateUser_ReturnsOk_WhenValidRequest()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var request = new UpdateUserRequest { FullName = "Updated Name" };
        var updatedUser = new UserDto { UserId = 1, FullName = "Updated Name" };
        _serviceMock.Setup(s => s.UpdateUserAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.UpdateUser(1, request, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(updatedUser);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task UpdateUser_ReturnsOk_WhenUserUpdatesThemselves()
    {
        // Arrange
        _controller.ControllerContext = UserContext(5);
        var request = new UpdateUserRequest { FullName = "My Updated Name" };
        var updatedUser = new UserDto { UserId = 5, FullName = "My Updated Name" };
        _serviceMock.Setup(s => s.UpdateUserAsync(5, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.UpdateUser(5, request, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task UpdateUser_ReturnsForbidden_WhenUserTriesToUpdateOtherUser()
    {
        // Arrange
        _controller.ControllerContext = UserContext(5);
        var request = new UpdateUserRequest { FullName = "Updated Name" };

        // Act
        var result = await _controller.UpdateUser(10, request, default);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        statusCodeResult.StatusCode.Should().Be(403);
        _serviceMock.Verify(s => s.UpdateUserAsync(It.IsAny<int>(), It.IsAny<UpdateUserRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUser_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        _controller.ModelState.AddModelError("Email", "Invalid email");
        var request = new UpdateUserRequest();

        // Act
        var result = await _controller.UpdateUser(1, request, default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        badRequestResult.StatusCode.Should().Be(400);
        _serviceMock.Verify(s => s.UpdateUserAsync(It.IsAny<int>(), It.IsAny<UpdateUserRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUser_ReturnsNotFound_WhenUserNotExists()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var request = new UpdateUserRequest { FullName = "Updated Name" };
        _serviceMock.Setup(s => s.UpdateUserAsync(999, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.UpdateUser(999, request, default);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        notFoundResult.StatusCode.Should().Be(404);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task UpdateUser_ReturnsBadRequest_WhenInvalidOperationException()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var request = new UpdateUserRequest { Phone = "0909123456" };
        _serviceMock.Setup(s => s.UpdateUserAsync(1, request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Số điện thoại đã được sử dụng."));

        // Act
        var result = await _controller.UpdateUser(1, request, default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        badRequestResult.StatusCode.Should().Be(400);
        _serviceMock.VerifyAll();
    }

    // ===== DeleteUser Tests =====

    [Fact]
    public async Task DeleteUser_ReturnsNoContent_WhenUserDeleted()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        _serviceMock.Setup(s => s.DeleteUserAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteUser(1, default);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task DeleteUser_ReturnsNotFound_WhenUserNotExists()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        _serviceMock.Setup(s => s.DeleteUserAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteUser(999, default);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        notFoundResult.StatusCode.Should().Be(404);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task DeleteUser_ReturnsConflict_WhenInvalidOperationException()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        _serviceMock.Setup(s => s.DeleteUserAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Không thể xóa vì bệnh nhân còn dữ liệu liên quan."));

        // Act
        var result = await _controller.DeleteUser(1, default);

        // Assert
        var conflictResult = Assert.IsType<ObjectResult>(result);
        conflictResult.StatusCode.Should().Be(409);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task DeleteUser_ReturnsInternalServerError_WhenException()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        _serviceMock.Setup(s => s.DeleteUserAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.DeleteUser(1, default);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        statusCodeResult.StatusCode.Should().Be(500);
        _serviceMock.VerifyAll();
    }

    // ===== ToggleUserStatus Tests =====

    [Fact]
    public async Task ToggleUserStatus_ReturnsOk_WhenStatusToggled()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        _serviceMock.Setup(s => s.ToggleUserStatusAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var updatedUser = new UserDto { UserId = 1, IsActive = false };
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.ToggleUserStatus(1, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(updatedUser);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task ToggleUserStatus_ReturnsNotFound_WhenUserNotExists()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        _serviceMock.Setup(s => s.ToggleUserStatusAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ToggleUserStatus(999, default);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        notFoundResult.StatusCode.Should().Be(404);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task ToggleUserStatus_ReturnsInternalServerError_WhenException()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        _serviceMock.Setup(s => s.ToggleUserStatusAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.ToggleUserStatus(1, default);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        statusCodeResult.StatusCode.Should().Be(500);
        _serviceMock.VerifyAll();
    }

    // ===== SearchUsers Tests =====

    [Fact]
    public async Task SearchUsers_ReturnsOk_WithResults()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var request = new SearchUserRequest { FullName = "Test", PageNumber = 1, PageSize = 10 };
        var response = new SearchUserResponse
        {
            Users = new List<UserDto>
            {
                new UserDto { UserId = 1, FullName = "Test User" }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };
        _serviceMock.Setup(s => s.SearchUsersAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.SearchUsers(request, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(response);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task SearchUsers_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        _controller.ModelState.AddModelError("PageNumber", "Invalid page number");
        var request = new SearchUserRequest();

        // Act
        var result = await _controller.SearchUsers(request, default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        badRequestResult.StatusCode.Should().Be(400);
        _serviceMock.Verify(s => s.SearchUsersAsync(It.IsAny<SearchUserRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchUsers_ReturnsInternalServerError_WhenException()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var request = new SearchUserRequest { PageNumber = 1, PageSize = 10 };
        _serviceMock.Setup(s => s.SearchUsersAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.SearchUsers(request, default);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        statusCodeResult.StatusCode.Should().Be(500);
        _serviceMock.VerifyAll();
    }

    // ===== GetAllPatients Tests =====

    [Fact]
    public async Task GetAllPatients_ReturnsOk_WithPatients()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var patients = new List<UserDto>
        {
            new UserDto { UserId = 1, FullName = "Patient 1", Role = "Patient" },
            new UserDto { UserId = 2, FullName = "Patient 2", Role = "Patient" }
        };
        _serviceMock.Setup(s => s.GetAllPatientsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(patients);

        // Act
        var result = await _controller.GetAllPatients(default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(patients);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetAllPatients_ReturnsInternalServerError_WhenException()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        _serviceMock.Setup(s => s.GetAllPatientsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllPatients(default);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        statusCodeResult.StatusCode.Should().Be(500);
        _serviceMock.VerifyAll();
    }
}

