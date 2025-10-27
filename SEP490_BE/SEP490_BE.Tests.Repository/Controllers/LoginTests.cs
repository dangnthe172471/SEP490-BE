using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.Tests.Controllers
{
    public class LoginTests
    {
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<IConfiguration> _configurationMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IResetTokenService> _resetTokenServiceMock = new();
        private readonly Mock<IEmailService> _emailServiceMock = new();

        private AuthController MakeController()
        {
            return new AuthController(
                _userServiceMock.Object,
                _configurationMock.Object,
                _userRepositoryMock.Object,
                _resetTokenServiceMock.Object,
                _emailServiceMock.Object
            );
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnLOGIN_SUCCESS()
        {
            // Arrange - Test Case 1: 0905123456 + 123456 = LOGIN_SUCCESS
            var request = new LoginRequest { Phone = "0905123456", Password = "123456" };
            var userDto = new UserDto
            {
                UserId = 1,
                Phone = "0905123456",
                FullName = "Test User",
                Email = "test@example.com",
                Role = "Patient",
                Gender = "Male",
                Dob = new DateOnly(1990, 1, 1),
                IsActive = true
            };

            // Mock user exists and is active
            var user = new User { UserId = 1, Phone = "0905123456", IsActive = true };
            _userRepositoryMock.Setup(r => r.GetByPhoneAsync(request.Phone, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Mock password validation succeeds
            _userServiceMock.Setup(s => s.ValidateUserAsync(request.Phone, request.Password, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userDto);

            // Mock JWT configuration
            var jwtSectionMock = new Mock<IConfigurationSection>();
            jwtSectionMock.Setup(x => x["Key"]).Returns("YourSuperSecretKeyThatIsAtLeast32CharactersLong!");
            jwtSectionMock.Setup(x => x["Issuer"]).Returns("TestIssuer");
            jwtSectionMock.Setup(x => x["Audience"]).Returns("TestAudience");
            
            _configurationMock.Setup(c => c.GetSection("Jwt")).Returns(jwtSectionMock.Object);

            var controller = MakeController();

            // Act
            var result = await controller.Login(request, CancellationToken.None);

            // Assert - LOGIN_SUCCESS
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Login_WithNonExistentUser_ShouldReturnLOGIN_FAILED()
        {
            // Arrange - Test Case 2: 0960900476 + 123456 = LOGIN_FAILED (user not found)
            var request = new LoginRequest { Phone = "0960900476", Password = "123456" };
            _userRepositoryMock.Setup(r => r.GetByPhoneAsync(request.Phone, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var controller = MakeController();

            // Act
            var result = await controller.Login(request, CancellationToken.None);

            // Assert - LOGIN_FAILED
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Login_WithInactiveUser_ShouldReturnLOGIN_FAILED()
        {
            // Arrange - Test Case 3: 0962900476 + 123456 = LOGIN_FAILED (user inactive)
            var request = new LoginRequest { Phone = "0962900476", Password = "123456" };
            var inactiveUser = new User { UserId = 1, Phone = "0962900476", IsActive = false };
            _userRepositoryMock.Setup(r => r.GetByPhoneAsync(request.Phone, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inactiveUser);

            var controller = MakeController();

            // Act
            var result = await controller.Login(request, CancellationToken.None);

            // Assert - LOGIN_FAILED
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Login_WithWrongPassword_ShouldReturnLOGIN_FAILED()
        {
            // Arrange - Test Case 4: 0905123456 + 123654 = LOGIN_FAILED (wrong password)
            var request = new LoginRequest { Phone = "0905123456", Password = "123654" };
            var user = new User { UserId = 1, Phone = "0905123456", IsActive = true };
            _userRepositoryMock.Setup(r => r.GetByPhoneAsync(request.Phone, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _userServiceMock.Setup(s => s.ValidateUserAsync(request.Phone, request.Password, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserDto?)null); // Password validation fails

            var controller = MakeController();

            // Act
            var result = await controller.Login(request, CancellationToken.None);

            // Assert - LOGIN_FAILED
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult!.Value.Should().NotBeNull();
        }
    }
}
