using FluentAssertions;
using Moq;
using SEP490_BE.BLL.Services;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _repo = new();

        // Helper: Tạo User entity
        private static User MakeUser(int id, string phone, string fullName, bool isActive = true, int roleId = 2)
            => new User
            {
                UserId = id,
                Phone = phone,
                FullName = fullName,
                Email = "test@example.com",
                Gender = "Male",
                Dob = new DateOnly(1990, 1, 1),
                IsActive = isActive,
                RoleId = roleId,
                Role = new Role { RoleId = roleId, RoleName = roleId == 2 ? "Patient" : "Doctor" }
            };

        // Helper: Tạo UserDto
        private static UserDto MakeUserDto(int id, string phone, string fullName, bool isActive = true, string role = "Patient")
            => new UserDto
            {
                UserId = id,
                Phone = phone,
                FullName = fullName,
                Email = "test@example.com",
                Gender = "Male",
                Dob = new DateOnly(1990, 1, 1),
                IsActive = isActive,
                Role = role
            };

        // ===== LOGIN TESTS =====

        [Fact]
        public async Task ValidateUserAsync_ReturnsUserDto_WhenValidCredentials()
        {
            // Arrange
            var phone = "0905123456";
            var password = "123456";
            var user = MakeUser(1, phone, "Test User");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            _repo.Setup(r => r.GetByPhoneAsync(phone, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var service = new UserService(_repo.Object);

            // Act
            var result = await service.ValidateUserAsync(phone, password, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(1);
            result.Phone.Should().Be(phone);
            result.FullName.Should().Be("Test User");
            result.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateUserAsync_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            var phone = "0960900476";
            var password = "123456";

            _repo.Setup(r => r.GetByPhoneAsync(phone, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var service = new UserService(_repo.Object);

            // Act
            var result = await service.ValidateUserAsync(phone, password, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateUserAsync_ReturnsNull_WhenUserInactive()
        {
            // Arrange
            var phone = "0962900476";
            var password = "123456";
            var user = MakeUser(1, phone, "Test User", isActive: false);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            _repo.Setup(r => r.GetByPhoneAsync(phone, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var service = new UserService(_repo.Object);

            // Act
            var result = await service.ValidateUserAsync(phone, password, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateUserAsync_ReturnsNull_WhenWrongPassword()
        {
            // Arrange
            var phone = "0905123456";
            var password = "123456";
            var wrongPassword = "123654";
            var user = MakeUser(1, phone, "Test User");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            _repo.Setup(r => r.GetByPhoneAsync(phone, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var service = new UserService(_repo.Object);

            // Act
            var result = await service.ValidateUserAsync(phone, wrongPassword, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        // ===== CHANGE PASSWORD TESTS =====

        [Fact]
        public async Task UpdatePasswordAsync_ReturnsTrue_WhenValidUserAndPassword()
        {
            // Arrange
            var userId = 1;
            var newPassword = "newPassword456";
            var user = MakeUser(userId, "0905123456", "Test User");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldPassword123");

            _repo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _repo.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = new UserService(_repo.Object);

            // Act
            var result = await service.UpdatePasswordAsync(userId, newPassword, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            _repo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePasswordAsync_ReturnsFalse_WhenUserNotFound()
        {
            // Arrange
            var userId = 999;
            var newPassword = "newPassword456";

            _repo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var service = new UserService(_repo.Object);

            // Act
            var result = await service.UpdatePasswordAsync(userId, newPassword, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _repo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePasswordAsync_ThrowsException_WhenRepositoryThrows()
        {
            // Arrange
            var userId = 1;
            var newPassword = "newPassword456";
            var user = MakeUser(userId, "0905123456", "Test User");

            _repo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _repo.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var service = new UserService(_repo.Object);

            // Act & Assert
            await service.Invoking(s => s.UpdatePasswordAsync(userId, newPassword, CancellationToken.None))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Database error");
        }

        [Fact]
        public async Task UpdatePasswordAsync_UpdatesPasswordHash_WhenValidInput()
        {
            // Arrange
            var userId = 1;
            var oldPassword = "oldPassword123";
            var newPassword = "newPassword456";
            var user = MakeUser(userId, "0905123456", "Test User");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(oldPassword);

            _repo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            User? updatedUser = null;
            _repo.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Callback<User, CancellationToken>((u, ct) => updatedUser = u)
                .Returns(Task.CompletedTask);

            var service = new UserService(_repo.Object);

            // Act
            var result = await service.UpdatePasswordAsync(userId, newPassword, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            updatedUser.Should().NotBeNull();
            BCrypt.Net.BCrypt.Verify(newPassword, updatedUser!.PasswordHash).Should().BeTrue();
            BCrypt.Net.BCrypt.Verify(oldPassword, updatedUser.PasswordHash).Should().BeFalse();
        }
    }
}