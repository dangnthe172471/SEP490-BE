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
    private readonly Mock<IRoomService> _roomServiceMock = new(MockBehavior.Strict);
    private readonly AdministratorController _controller;

    public AdministratorControllerTests()
    {
        _controller = new AdministratorController(_serviceMock.Object, _roomServiceMock.Object);
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

    private static UserDto CreateCompleteUserDto(int userId, string fullName, string phone, string? role = "Patient", bool isActive = true)
    {
        return new UserDto
        {
            UserId = userId,
            Phone = phone,
            FullName = fullName,
            Email = $"user{userId}@example.com",
            Role = role,
            Gender = "Male",
            Dob = DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
            IsActive = isActive,
            EmailVerified = true,
            Avatar = $"avatar{userId}.jpg",
            Allergies = "None",
            MedicalHistory = "No history"
        };
    }

    // ===== GetAll Tests =====

    [Fact]
    public async Task GetAll_UTCID01_ReturnsOk_WithUsers()
    {
        // Arrange
        var users = new List<UserDto>
        {
            CreateCompleteUserDto(1, "User 1", "0909123456", "Patient"),
            CreateCompleteUserDto(2, "User 2", "0909123457", "Doctor")
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
    public async Task GetAll_UTCID02_ReturnsOk_WithEmptyList()
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
    public async Task GetById_UTCID01_ReturnsOk_WhenUserExists()
    {
        // Arrange
        var user = CreateCompleteUserDto(1, "Test User", "0909123456", "Patient");
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
    public async Task GetById_UTCID02_ReturnsNotFound_WhenUserNotExists()
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

    [Fact]
    public async Task GetById_UTCID03_ReturnsOk_WhenUserIdIsZero()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(0, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.GetById(0, default);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
        notFoundResult.StatusCode.Should().Be(404);
        _serviceMock.VerifyAll();
    }

    // ===== CreateUser Tests =====

    [Fact]
    public async Task CreateUser_UTCID01_ReturnsCreated_WhenValidRequest()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var request = new CreateUserRequest
        {
            Phone = "0909123456",
            Password = "123456",
            FullName = "New User",
            Email = "newuser@example.com",
            Dob = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            Gender = "Male",
            RoleId = 2
        };
        var createdUser = CreateCompleteUserDto(100, request.FullName, request.Phone, "Patient");
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
    public async Task CreateUser_UTCID02_ReturnsCreated_WhenValidRequestWithPatientFields()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var request = new CreateUserRequest
        {
            Phone = "0909123457",
            Password = "123456",
            FullName = "New Patient",
            Email = "patient@example.com",
            Dob = DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
            Gender = "Female",
            RoleId = 3,
            Allergies = "Peanuts",
            MedicalHistory = "Diabetes"
        };
        var createdUser = CreateCompleteUserDto(101, request.FullName, request.Phone, "Patient");
        createdUser.Allergies = request.Allergies;
        createdUser.MedicalHistory = request.MedicalHistory;
        _serviceMock.Setup(s => s.CreateUserAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateUser(request, default);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        createdResult.StatusCode.Should().Be(201);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task CreateUser_UTCID03_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        _controller.ModelState.AddModelError("Phone", "Số điện thoại là bắt buộc");
        var request = new CreateUserRequest();

        // Act
        var result = await _controller.CreateUser(request, default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        badRequestResult.StatusCode.Should().Be(400);
        _serviceMock.Verify(s => s.CreateUserAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateUser_UTCID04_ReturnsBadRequest_WhenPhoneIsEmpty()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var request = new CreateUserRequest
        {
            Phone = "",
            Password = "123456",
            FullName = "New User",
            RoleId = 2
        };
        _controller.ModelState.AddModelError("Phone", "Số điện thoại là bắt buộc");

        // Act
        var result = await _controller.CreateUser(request, default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        badRequestResult.StatusCode.Should().Be(400);
        _serviceMock.Verify(s => s.CreateUserAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateUser_UTCID05_ReturnsBadRequest_WhenPasswordIsTooShort()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var request = new CreateUserRequest
        {
            Phone = "0909123456",
            Password = "12345",
            FullName = "New User",
            RoleId = 2
        };
        _controller.ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 6 ký tự");

        // Act
        var result = await _controller.CreateUser(request, default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        badRequestResult.StatusCode.Should().Be(400);
        _serviceMock.Verify(s => s.CreateUserAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateUser_UTCID06_ReturnsBadRequest_WhenServiceReturnsNull()
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
    public async Task CreateUser_UTCID07_ReturnsBadRequest_WhenInvalidOperationException()
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
    public async Task CreateUser_UTCID08_ReturnsInternalServerError_WhenException()
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
    public async Task UpdateUser_UTCID01_ReturnsOk_WhenAdminUpdatesUser()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var request = new UpdateUserRequest { FullName = "Updated Name", Email = "updated@example.com" };
        var updatedUser = CreateCompleteUserDto(1, "Updated Name", "0909123456", "Patient");
        updatedUser.Email = "updated@example.com";
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
    public async Task UpdateUser_UTCID02_ReturnsOk_WhenUserUpdatesThemselves()
    {
        // Arrange
        _controller.ControllerContext = UserContext(5);
        var request = new UpdateUserRequest { FullName = "My Updated Name", Phone = "0909999999" };
        var updatedUser = CreateCompleteUserDto(5, "My Updated Name", "0909999999", "Patient");
        _serviceMock.Setup(s => s.UpdateUserAsync(5, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.UpdateUser(5, request, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(updatedUser);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task UpdateUser_UTCID03_ReturnsForbidden_WhenUserTriesToUpdateOtherUser()
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
    public async Task UpdateUser_UTCID04_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        _controller.ModelState.AddModelError("Email", "Email không hợp lệ");
        var request = new UpdateUserRequest { Email = "invalid-email" };

        // Act
        var result = await _controller.UpdateUser(1, request, default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        badRequestResult.StatusCode.Should().Be(400);
        _serviceMock.Verify(s => s.UpdateUserAsync(It.IsAny<int>(), It.IsAny<UpdateUserRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUser_UTCID05_ReturnsNotFound_WhenUserNotExists()
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
    public async Task UpdateUser_UTCID06_ReturnsBadRequest_WhenInvalidOperationException()
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

    [Fact]
    public async Task UpdateUser_UTCID07_ReturnsInternalServerError_WhenException()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var request = new UpdateUserRequest { FullName = "Updated Name" };
        _serviceMock.Setup(s => s.UpdateUserAsync(1, request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.UpdateUser(1, request, default);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        statusCodeResult.StatusCode.Should().Be(500);
        _serviceMock.VerifyAll();
    }

    // ===== DeleteUser Tests =====

    [Fact]
    public async Task DeleteUser_UTCID01_ReturnsNoContent_WhenUserDeleted()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        _serviceMock.Setup(s => s.DeleteUserAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteUser(1, default);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        noContentResult.StatusCode.Should().Be(204);
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task DeleteUser_UTCID02_ReturnsNotFound_WhenUserNotExists()
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
    public async Task DeleteUser_UTCID03_ReturnsConflict_WhenInvalidOperationException()
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
    public async Task DeleteUser_UTCID04_ReturnsInternalServerError_WhenException()
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
    public async Task ToggleUserStatus_UTCID01_ReturnsOk_WhenStatusToggled()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        _serviceMock.Setup(s => s.ToggleUserStatusAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var updatedUser = CreateCompleteUserDto(1, "Test User", "0909123456", "Patient", false);
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
    public async Task ToggleUserStatus_UTCID02_ReturnsNotFound_WhenUserNotExists()
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
    public async Task ToggleUserStatus_UTCID03_ReturnsInternalServerError_WhenException()
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
    public async Task SearchUsers_UTCID01_ReturnsOk_WithResults()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var request = new SearchUserRequest { FullName = "Test", PageNumber = 1, PageSize = 10 };
        var response = new SearchUserResponse
        {
            Users = new List<UserDto>
            {
                CreateCompleteUserDto(1, "Test User", "0909123456", "Patient")
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
    public async Task SearchUsers_UTCID02_ReturnsOk_WithEmptyResults()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var request = new SearchUserRequest { FullName = "NonExistent", PageNumber = 1, PageSize = 10 };
        var response = new SearchUserResponse
        {
            Users = new List<UserDto>(),
            TotalCount = 0,
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
        ((SearchUserResponse)okResult.Value!).Users.Should().BeEmpty();
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task SearchUsers_UTCID03_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        _controller.ModelState.AddModelError("PageNumber", "Invalid page number");
        var request = new SearchUserRequest { PageNumber = -1, PageSize = 10 };

        // Act
        var result = await _controller.SearchUsers(request, default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        badRequestResult.StatusCode.Should().Be(400);
        _serviceMock.Verify(s => s.SearchUsersAsync(It.IsAny<SearchUserRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchUsers_UTCID04_ReturnsInternalServerError_WhenException()
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
    public async Task GetAllPatients_UTCID01_ReturnsOk_WithPatients()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        var patients = new List<UserDto>
        {
            CreateCompleteUserDto(1, "Patient 1", "0909123456", "Patient"),
            CreateCompleteUserDto(2, "Patient 2", "0909123457", "Patient")
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
    public async Task GetAllPatients_UTCID02_ReturnsOk_WithEmptyList()
    {
        // Arrange
        _controller.ControllerContext = AdminContext();
        _serviceMock.Setup(s => s.GetAllPatientsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserDto>());

        // Act
        var result = await _controller.GetAllPatients(default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        okResult.StatusCode.Should().Be(200);
        ((IEnumerable<UserDto>)okResult.Value!).Should().BeEmpty();
        _serviceMock.VerifyAll();
    }

    [Fact]
    public async Task GetAllPatients_UTCID03_ReturnsInternalServerError_WhenException()
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
