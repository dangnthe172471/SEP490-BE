using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SEP490_BE.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UsersController _controller;

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            _controller = new UsersController(_userServiceMock.Object);
        }

        private UsersController CreateControllerWithUser(int userId, string role)
        {
            var controller = new UsersController(_userServiceMock.Object);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            };
            return controller;
        }

        [Fact]
        public async Task CreateUser_ShouldReturnCreatedResult_WhenSuccessful()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Phone = "0909123456",
                Password = "password123",
                FullName = "Test User",
                RoleId = 1
            };
            var userDto = new UserDto { UserId = 1, FullName = "Test User" };
            _userServiceMock
                .Setup(s => s.CreateUserAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userDto);

            // Act
            var result = await _controller.CreateUser(request, CancellationToken.None);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<UserDto>(createdResult.Value);
            Assert.Equal(1, returnValue.UserId);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenPhoneAlreadyExists()
        {
            // Arrange
            var request = new CreateUserRequest { Phone = "0909123456" };
            _userServiceMock
                .Setup(s => s.CreateUserAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Số điện thoại đã được sử dụng."));

            // Act
            var result = await _controller.CreateUser(request, CancellationToken.None);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Số điện thoại đã được sử dụng", badRequest.Value?.ToString() ?? "");
        }

        [Fact]
        public async Task CreateUser_ShouldReturn500_WhenUnhandledExceptionOccurs()
        {
            // Arrange
            var request = new CreateUserRequest();
            _userServiceMock
                .Setup(s => s.CreateUserAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.CreateUser(request, CancellationToken.None);

            // Assert
            var objResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objResult.StatusCode);
        }

        // ===============================================================
        //                    GET ALL TEST CASES (2)
        // ===============================================================

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithUsers()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { UserId = 1, FullName = "User 1" },
                new UserDto { UserId = 2, FullName = "User 2" }
            };
            _userServiceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetAll(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<UserDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            _userServiceMock.VerifyAll();
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithEmptyList()
        {
            // Arrange
            _userServiceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserDto>());

            // Act
            var result = await _controller.GetAll(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<UserDto>>(okResult.Value);
            Assert.Empty(returnValue);
            _userServiceMock.VerifyAll();
        }

        // ===============================================================
        //                    GET BY ID TEST CASES (2)
        // ===============================================================

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenFound()
        {
            // Arrange
            var userId = 1;
            var userDto = new UserDto { UserId = userId, FullName = "Test User" };
            _userServiceMock
                .Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userDto);

            // Act
            var result = await _controller.GetById(userId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(userId, returnValue.UserId);
            _userServiceMock.VerifyAll();
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenNotFound()
        {
            // Arrange
            var userId = 999;
            _userServiceMock
                .Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserDto?)null);

            // Act
            var result = await _controller.GetById(userId, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
            _userServiceMock.VerifyAll();
        }

        // ===============================================================
        //                    GET ALL2 (TEST-SECURE) TEST CASES (2)
        // ===============================================================

        [Fact]
        public async Task GetAll2_ShouldReturnOk_WhenAuthorizedAsDoctor()
        {
            // Arrange
            var controller = CreateControllerWithUser(1, "Doctor");
            var users = new List<UserDto>
            {
                new UserDto { UserId = 1, FullName = "User 1" }
            };
            _userServiceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(users);

            // Act
            var result = await controller.GetAll2(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<UserDto>>(okResult.Value);
            Assert.Single(returnValue);
            _userServiceMock.VerifyAll();
        }

        // ===============================================================
        //                    UPDATE USER TEST CASES (6)
        // ===============================================================

        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenAdminUpdatesAnyUser()
        {
            // Arrange
            var controller = CreateControllerWithUser(1, "Administrator");
            var userId = 999;
            var request = new UpdateUserRequest { FullName = "Updated Name" };
            var userDto = new UserDto { UserId = userId, FullName = "Updated Name" };

            _userServiceMock
                .Setup(s => s.UpdateUserAsync(userId, request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userDto);

            // Act
            var result = await controller.UpdateUser(userId, request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal("Updated Name", returnValue.FullName);
            _userServiceMock.VerifyAll();
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenUserUpdatesThemselves()
        {
            // Arrange
            var userId = 1;
            var controller = CreateControllerWithUser(userId, "Patient");
            var request = new UpdateUserRequest { FullName = "Updated Name" };
            var userDto = new UserDto { UserId = userId, FullName = "Updated Name" };

            _userServiceMock
                .Setup(s => s.UpdateUserAsync(userId, request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userDto);

            // Act
            var result = await controller.UpdateUser(userId, request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal("Updated Name", returnValue.FullName);
            _userServiceMock.VerifyAll();
        }

        [Fact]
        public async Task UpdateUser_ShouldReturn403_WhenUserUpdatesOtherUser()
        {
            // Arrange
            var currentUserId = 1;
            var targetUserId = 999;
            var controller = CreateControllerWithUser(currentUserId, "Patient");
            var request = new UpdateUserRequest { FullName = "Updated Name" };

            // Act
            var result = await controller.UpdateUser(targetUserId, request, CancellationToken.None);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(403, statusCodeResult.StatusCode);
            _userServiceMock.Verify(s => s.UpdateUserAsync(It.IsAny<int>(), It.IsAny<UpdateUserRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnNotFound_WhenUserNotFound()
        {
            // Arrange
            var controller = CreateControllerWithUser(1, "Administrator");
            var userId = 999;
            var request = new UpdateUserRequest { FullName = "Updated Name" };

            _userServiceMock
                .Setup(s => s.UpdateUserAsync(userId, request, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserDto?)null);

            // Act
            var result = await controller.UpdateUser(userId, request, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            _userServiceMock.VerifyAll();
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenInvalidOperationException()
        {
            // Arrange
            var controller = CreateControllerWithUser(1, "Administrator");
            var userId = 1;
            var request = new UpdateUserRequest { FullName = "Updated Name" };

            _userServiceMock
                .Setup(s => s.UpdateUserAsync(userId, request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Invalid operation"));

            // Act
            var result = await controller.UpdateUser(userId, request, CancellationToken.None);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            _userServiceMock.VerifyAll();
        }

        [Fact]
        public async Task UpdateUser_ShouldReturn500_WhenException()
        {
            // Arrange
            var controller = CreateControllerWithUser(1, "Administrator");
            var userId = 1;
            var request = new UpdateUserRequest { FullName = "Updated Name" };

            _userServiceMock
                .Setup(s => s.UpdateUserAsync(userId, request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await controller.UpdateUser(userId, request, CancellationToken.None);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _userServiceMock.VerifyAll();
        }

        // ===============================================================
        //                    DELETE USER TEST CASES (4)
        // ===============================================================

        [Fact]
        public async Task DeleteUser_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            var controller = CreateControllerWithUser(1, "Administrator");
            var userId = 999;

            _userServiceMock
                .Setup(s => s.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await controller.DeleteUser(userId, CancellationToken.None);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            _userServiceMock.VerifyAll();
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNotFound_WhenUserNotFound()
        {
            // Arrange
            var controller = CreateControllerWithUser(1, "Administrator");
            var userId = 999;

            _userServiceMock
                .Setup(s => s.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await controller.DeleteUser(userId, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            _userServiceMock.VerifyAll();
        }

        [Fact]
        public async Task DeleteUser_ShouldReturn409_WhenInvalidOperationException()
        {
            // Arrange
            var controller = CreateControllerWithUser(1, "Administrator");
            var userId = 999;

            _userServiceMock
                .Setup(s => s.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Cannot delete user with dependencies"));

            // Act
            var result = await controller.DeleteUser(userId, CancellationToken.None);

            // Assert
            var conflictResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);
            _userServiceMock.VerifyAll();
        }

        [Fact]
        public async Task DeleteUser_ShouldReturn500_WhenException()
        {
            // Arrange
            var controller = CreateControllerWithUser(1, "Administrator");
            var userId = 999;

            _userServiceMock
                .Setup(s => s.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await controller.DeleteUser(userId, CancellationToken.None);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _userServiceMock.VerifyAll();
        }

        // ===============================================================
        //                    TOGGLE USER STATUS TEST CASES (3)
        // ===============================================================

        [Fact]
        public async Task ToggleUserStatus_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var controller = CreateControllerWithUser(1, "Administrator");
            var userId = 999;
            var userDto = new UserDto { UserId = userId, IsActive = false };

            _userServiceMock
                .Setup(s => s.ToggleUserStatusAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _userServiceMock
                .Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userDto);

            // Act
            var result = await controller.ToggleUserStatus(userId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(userId, returnValue.UserId);
            _userServiceMock.VerifyAll();
        }

        [Fact]
        public async Task ToggleUserStatus_ShouldReturnNotFound_WhenUserNotFound()
        {
            // Arrange
            var controller = CreateControllerWithUser(1, "Administrator");
            var userId = 999;

            _userServiceMock
                .Setup(s => s.ToggleUserStatusAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await controller.ToggleUserStatus(userId, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            _userServiceMock.VerifyAll();
        }

        [Fact]
        public async Task ToggleUserStatus_ShouldReturn500_WhenException()
        {
            // Arrange
            var controller = CreateControllerWithUser(1, "Administrator");
            var userId = 999;

            _userServiceMock
                .Setup(s => s.ToggleUserStatusAsync(userId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await controller.ToggleUserStatus(userId, CancellationToken.None);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _userServiceMock.VerifyAll();
        }

        // ===============================================================
        //                    SEARCH USERS TEST CASES (3)
        // ===============================================================

        [Fact]
        public async Task SearchUsers_ShouldReturnOk_WithResults()
        {
            // Arrange
            var controller = CreateControllerWithUser(1, "Administrator");
            var request = new SearchUserRequest
            {
                FullName = "Test",
                PageNumber = 1,
                PageSize = 10
            };
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

            _userServiceMock
                .Setup(s => s.SearchUsersAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await controller.SearchUsers(request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<SearchUserResponse>(okResult.Value);
            Assert.Equal(1, returnValue.TotalCount);
            _userServiceMock.VerifyAll();
        }

        [Fact]
        public async Task SearchUsers_ShouldReturn500_WhenException()
        {
            // Arrange
            var controller = CreateControllerWithUser(1, "Administrator");
            var request = new SearchUserRequest();

            _userServiceMock
                .Setup(s => s.SearchUsersAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await controller.SearchUsers(request, CancellationToken.None);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _userServiceMock.VerifyAll();
        }

        // ===============================================================
        //                    GET ALL PATIENTS TEST CASES (2)
        // ===============================================================

        [Fact]
        public async Task GetAllPatients_ShouldReturnOk_WithPatients()
        {
            // Arrange
            var controller = CreateControllerWithUser(1, "Administrator");
            var patients = new List<UserDto>
            {
                new UserDto { UserId = 1, FullName = "Patient 1", Role = "Patient" },
                new UserDto { UserId = 2, FullName = "Patient 2", Role = "Patient" }
            };

            _userServiceMock
                .Setup(s => s.GetAllPatientsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(patients);

            // Act
            var result = await controller.GetAllPatients(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<UserDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            _userServiceMock.VerifyAll();
        }

        [Fact]
        public async Task GetAllPatients_ShouldReturn500_WhenException()
        {
            // Arrange
            var controller = CreateControllerWithUser(1, "Administrator");

            _userServiceMock
                .Setup(s => s.GetAllPatientsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await controller.GetAllPatients(CancellationToken.None);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _userServiceMock.VerifyAll();
        }
    }
}
