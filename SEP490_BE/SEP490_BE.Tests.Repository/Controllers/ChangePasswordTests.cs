using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers
{
    public class ChangePasswordTests
    {
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<IConfiguration> _configurationMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IResetTokenService> _resetTokenServiceMock = new();
        private readonly Mock<IEmailService> _emailServiceMock = new();

        private AuthController MakeControllerWithUser(int? userId = 1, string? role = "Patient")
        {
            var controller = new AuthController(
                _userServiceMock.Object,
                _configurationMock.Object,
                _userRepositoryMock.Object,
                _resetTokenServiceMock.Object,
                _emailServiceMock.Object
            );

            var claims = new List<Claim>();
            if (userId.HasValue)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
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
        public async Task ChangePassword_WithValidCredentials_ShouldReturnSuccess()
        {
            // Arrange - Test Case: Valid old password + valid new password + matching confirm
            var request = new ChangePasswordRequest 
            { 
                CurrentPassword = "oldPassword123", 
                NewPassword = "newPassword456" 
            };
            
            var user = new User 
            { 
                UserId = 1, 
                Phone = "0905123456", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldPassword123"),
                IsActive = true 
            };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _userServiceMock.Setup(s => s.UpdatePasswordAsync(1, "newPassword456", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = MakeControllerWithUser();

            // Act
            var result = await controller.ChangePassword(request, CancellationToken.None);

            // Assert - Change password successfully
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task ChangePassword_WithWrongOldPassword_ShouldReturnFailure()
        {
            // Arrange - Test Case: Wrong old password + valid new password + matching confirm
            var request = new ChangePasswordRequest 
            { 
                CurrentPassword = "wrongOldPassword", 
                NewPassword = "newPassword456" 
            };
            
            var user = new User 
            { 
                UserId = 1, 
                Phone = "0905123456", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldPassword123"),
                IsActive = true 
            };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var controller = MakeControllerWithUser();

            // Act
            var result = await controller.ChangePassword(request, CancellationToken.None);

            // Assert - Failure
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task ChangePassword_WithNonExistentUser_ShouldReturnNotFound()
        {
            // Arrange - Test Case: User not found
            var request = new ChangePasswordRequest 
            { 
                CurrentPassword = "oldPassword123", 
                NewPassword = "newPassword456" 
            };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var controller = MakeControllerWithUser();

            // Act
            var result = await controller.ChangePassword(request, CancellationToken.None);

            // Assert - NotFoundException
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task ChangePassword_WithEmptyOldPassword_ShouldReturnBadRequest()
        {
            // Arrange - Test Case: Empty old password
            var request = new ChangePasswordRequest 
            { 
                CurrentPassword = "", 
                NewPassword = "newPassword456" 
            };

            var controller = MakeControllerWithUser();

            // Act
            var result = await controller.ChangePassword(request, CancellationToken.None);

            // Assert - BadRequest
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task ChangePassword_WithEmptyNewPassword_ShouldReturnBadRequest()
        {
            // Arrange - Test Case: Empty new password
            var request = new ChangePasswordRequest 
            { 
                CurrentPassword = "oldPassword123", 
                NewPassword = "" 
            };

            var controller = MakeControllerWithUser();

            // Act
            var result = await controller.ChangePassword(request, CancellationToken.None);

            // Assert - BadRequest
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task ChangePassword_WithShortNewPassword_ShouldReturnBadRequest()
        {
            // Arrange - Test Case: New password too short (< 6 characters)
            var request = new ChangePasswordRequest 
            { 
                CurrentPassword = "oldPassword123", 
                NewPassword = "12345" // Only 5 characters
            };

            var controller = MakeControllerWithUser();

            // Act
            var result = await controller.ChangePassword(request, CancellationToken.None);

            // Assert - Password length is invalid
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task ChangePassword_WithSameOldAndNewPassword_ShouldReturnBadRequest()
        {
            // Arrange - Test Case: Same old and new password
            var request = new ChangePasswordRequest 
            { 
                CurrentPassword = "oldPassword123", 
                NewPassword = "oldPassword123" 
            };

            var controller = MakeControllerWithUser();

            // Act
            var result = await controller.ChangePassword(request, CancellationToken.None);

            // Assert - BadRequest
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task ChangePassword_WithUpdatePasswordFailure_ShouldReturnBadRequest()
        {
            // Arrange - Test Case: UpdatePasswordAsync returns false
            var request = new ChangePasswordRequest 
            { 
                CurrentPassword = "oldPassword123", 
                NewPassword = "newPassword456" 
            };
            
            var user = new User 
            { 
                UserId = 1, 
                Phone = "0905123456", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldPassword123"),
                IsActive = true 
            };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _userServiceMock.Setup(s => s.UpdatePasswordAsync(1, "newPassword456", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // Update fails

            var controller = MakeControllerWithUser();

            // Act
            var result = await controller.ChangePassword(request, CancellationToken.None);

            // Assert - Failure
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task ChangePassword_WithInvalidUserId_ShouldReturnUnauthorized()
        {
            // Arrange - Test Case: Invalid user ID in claims
            var request = new ChangePasswordRequest 
            { 
                CurrentPassword = "oldPassword123", 
                NewPassword = "newPassword456" 
            };

            var controller = new AuthController(
                _userServiceMock.Object,
                _configurationMock.Object,
                _userRepositoryMock.Object,
                _resetTokenServiceMock.Object,
                _emailServiceMock.Object
            );

            // Set up empty user claims (no NameIdentifier)
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };

            // Act
            var result = await controller.ChangePassword(request, CancellationToken.None);

            // Assert - Unauthorized
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task ChangePassword_WithException_ShouldReturnInternalServerError()
        {
            // Arrange - Test Case: Exception during password change
            var request = new ChangePasswordRequest 
            { 
                CurrentPassword = "oldPassword123", 
                NewPassword = "newPassword456" 
            };
            
            var user = new User 
            { 
                UserId = 1, 
                Phone = "0905123456", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldPassword123"),
                IsActive = true 
            };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _userServiceMock.Setup(s => s.UpdatePasswordAsync(1, "newPassword456", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var controller = MakeControllerWithUser();

            // Act
            var result = await controller.ChangePassword(request, CancellationToken.None);

            // Assert - Internal Server Error
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task ChangePassword_WithValidCredentialsButServiceThrows_ShouldReturnInternalServerError()
        {
            // Arrange - Test Case: Valid credentials but service throws exception
            var request = new ChangePasswordRequest 
            { 
                CurrentPassword = "oldPassword123", 
                NewPassword = "newPassword456" 
            };
            
            var user = new User 
            { 
                UserId = 1, 
                Phone = "0905123456", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldPassword123"),
                IsActive = true 
            };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _userServiceMock.Setup(s => s.UpdatePasswordAsync(1, "newPassword456", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Service error"));

            var controller = MakeControllerWithUser();

            // Act
            var result = await controller.ChangePassword(request, CancellationToken.None);

            // Assert - Internal Server Error
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }
    }
}
