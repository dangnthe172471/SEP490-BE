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
    public class AuthControllerAdditionalTests
    {
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<IConfiguration> _configurationMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IResetTokenService> _resetTokenServiceMock = new();
        private readonly Mock<IEmailService> _emailServiceMock = new();

        private AuthController MakeController(int? userId = 1)
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

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"))
                }
            };
            return controller;
        }

        #region ForgotPassword Tests

        [Fact]
        public async Task ForgotPassword_WithValidEmail_ReturnsOk()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "test@example.com" };
            var user = new User { UserId = 1, Email = "test@example.com", FullName = "Test User" };
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _resetTokenServiceMock.Setup(s => s.StoreOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);
            _emailServiceMock.Setup(s => s.SendEmailAsync(request.Email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var controller = MakeController();

            // Act
            var result = await controller.ForgotPasswordByEmail(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _emailServiceMock.Verify(s => s.SendEmailAsync(request.Email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_WithNonExistentEmail_ReturnsOk()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "nonexistent@example.com" };
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var controller = MakeController();

            // Act
            var result = await controller.ForgotPasswordByEmail(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ForgotPassword_WithEmptyEmail_ReturnsBadRequest()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "" };

            var controller = MakeController();

            // Act
            var result = await controller.ForgotPasswordByEmail(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region VerifyEmailOtp Tests

        [Fact]
        public async Task VerifyEmailOtp_WithValidOtp_ReturnsOk()
        {
            // Arrange
            var request = new VerifyOtpRequest { Email = "test@example.com", OtpCode = "123456" };
            _resetTokenServiceMock.Setup(s => s.ValidateOtpAsync(request.Email, request.OtpCode))
                .ReturnsAsync(true);
            _resetTokenServiceMock.Setup(s => s.GenerateAndStoreTokenAsync(request.Email))
                .ReturnsAsync("reset-token-123");

            var controller = MakeController();

            // Act
            var result = await controller.VerifyEmailOtp(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task VerifyEmailOtp_WithInvalidOtp_ReturnsBadRequest()
        {
            // Arrange
            var request = new VerifyOtpRequest { Email = "test@example.com", OtpCode = "000000" };
            _resetTokenServiceMock.Setup(s => s.ValidateOtpAsync(request.Email, request.OtpCode))
                .ReturnsAsync(false);

            var controller = MakeController();

            // Act
            var result = await controller.VerifyEmailOtp(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region VerifyEmail Tests

        [Fact]
        public async Task VerifyEmail_WithValidOtp_ReturnsOk()
        {
            // Arrange
            var request = new VerifyOtpRequest { Email = "test@example.com", OtpCode = "123456" };
            _resetTokenServiceMock.Setup(s => s.ValidateOtpAsync(request.Email, request.OtpCode))
                .ReturnsAsync(true);
            _userServiceMock.Setup(s => s.VerifyEmailAsync(request.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = MakeController();

            // Act
            var result = await controller.VerifyEmail(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task VerifyEmail_WithInvalidOtp_ReturnsBadRequest()
        {
            // Arrange
            var request = new VerifyOtpRequest { Email = "test@example.com", OtpCode = "000000" };
            _resetTokenServiceMock.Setup(s => s.ValidateOtpAsync(request.Email, request.OtpCode))
                .ReturnsAsync(false);

            var controller = MakeController();

            // Act
            var result = await controller.VerifyEmail(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task VerifyEmail_WithEmptyEmail_ReturnsBadRequest()
        {
            // Arrange
            var request = new VerifyOtpRequest { Email = "", OtpCode = "123456" };

            var controller = MakeController();

            // Act
            var result = await controller.VerifyEmail(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region ResendVerificationEmail Tests

        [Fact]
        public async Task ResendVerificationEmail_WithValidEmail_ReturnsOk()
        {
            // Arrange
            var request = new ResendVerificationEmailRequest { Email = "test@example.com" };
            var user = new User { UserId = 1, Email = "test@example.com", FullName = "Test User", EmailVerified = false };
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _resetTokenServiceMock.Setup(s => s.GenerateOtpAsync(request.Email, It.IsAny<int>()))
                .ReturnsAsync("123456");
            _emailServiceMock.Setup(s => s.SendEmailAsync(request.Email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var controller = MakeController();

            // Act
            var result = await controller.ResendVerificationEmail(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ResendVerificationEmail_WithAlreadyVerified_ReturnsBadRequest()
        {
            // Arrange
            var request = new ResendVerificationEmailRequest { Email = "test@example.com" };
            var user = new User { UserId = 1, Email = "test@example.com", EmailVerified = true };
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var controller = MakeController();

            // Act
            var result = await controller.ResendVerificationEmail(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region ResetPassword Tests

        [Fact]
        public async Task ResetPassword_WithValidToken_ReturnsOk()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "test@example.com",
                Token = "valid-token",
                NewPassword = "newPassword123"
            };
            _resetTokenServiceMock.Setup(s => s.ValidateTokenAsync(request.Email, request.Token))
                .ReturnsAsync(true);
            _userServiceMock.Setup(s => s.ResetPasswordAsync(request.Email, request.NewPassword, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = MakeController();

            // Act
            var result = await controller.ResetPassword(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ResetPassword_WithInvalidToken_ReturnsBadRequest()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "test@example.com",
                Token = "invalid-token",
                NewPassword = "newPassword123"
            };
            _resetTokenServiceMock.Setup(s => s.ValidateTokenAsync(request.Email, request.Token))
                .ReturnsAsync(false);

            var controller = MakeController();

            // Act
            var result = await controller.ResetPassword(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region ChangeAvatar Tests

        [Fact]
        public async Task ChangeAvatar_WithValidFile_ReturnsOk()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("avatar.jpg");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var user = new User { UserId = 1, Avatar = null };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var controller = MakeController(1);

            // Act
            var result = await controller.ChangeAvatar(fileMock.Object, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ChangeAvatar_WithNullFile_ReturnsBadRequest()
        {
            // Arrange
            var controller = MakeController(1);

            // Act
            var result = await controller.ChangeAvatar(null!, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task ChangeAvatar_WithInvalidFileType_ReturnsBadRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("document.pdf");
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");
            fileMock.Setup(f => f.Length).Returns(1024);

            var controller = MakeController(1);

            // Act
            var result = await controller.ChangeAvatar(fileMock.Object, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task ChangeAvatar_WithFileTooLarge_ReturnsBadRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("avatar.jpg");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            fileMock.Setup(f => f.Length).Returns(6 * 1024 * 1024); // 6MB

            var controller = MakeController(1);

            // Act
            var result = await controller.ChangeAvatar(fileMock.Object, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task ChangeAvatar_WithInvalidUserId_ReturnsUnauthorized()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("avatar.jpg");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            fileMock.Setup(f => f.Length).Returns(1024);

            var controller = new AuthController(
                _userServiceMock.Object,
                _configurationMock.Object,
                _userRepositoryMock.Object,
                _resetTokenServiceMock.Object,
                _emailServiceMock.Object
            );
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };

            // Act
            var result = await controller.ChangeAvatar(fileMock.Object, CancellationToken.None);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        #endregion
    }
}

