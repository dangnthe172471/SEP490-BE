using FluentAssertions;
using Moq;
using SEP490_BE.BLL.Services;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace SEP490_BE.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            // Giả định UserService được khởi tạo với repository mock
            _userService = new UserService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrow_WhenPhoneExists()
        {
            // Arrange
            var request = new CreateUserRequest { Phone = "0909123456" };
            // Thiết lập mock để trả về một User khi GetByPhoneAsync được gọi
            _userRepositoryMock.Setup(r => r.GetByPhoneAsync(request.Phone, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new User { UserId = 1, Phone = request.Phone });

            // Act & Assert
            // Kiểm tra xem InvalidOperationException có được throw hay không
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.CreateUserAsync(request, CancellationToken.None));

            // Xác minh rằng phương thức AddAsync KHÔNG BAO GIỜ được gọi
            _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreatePatient_WhenRoleIsPatient()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Phone = "0909123456",
                Password = "abc123",
                RoleId = 2, // Patient role
                FullName = "Patient Test"
            };

            // Thiết lập mock: Giả định người dùng chưa tồn tại
            _userRepositoryMock.Setup(r => r.GetByPhoneAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync((User?)null);
            // Thiết lập mock: Trả về PatientId lớn nhất (để tính toán PatientId mới)
            _userRepositoryMock.Setup(r => r.GetMaxPatientIdAsync(It.IsAny<CancellationToken>()))
                               .ReturnsAsync(10); // Giả định PatientId lớn nhất là 10.

            // Act
            await _userService.CreateUserAsync(request, CancellationToken.None);

            // Assert
            // Xác minh rằng AddAsync được gọi với một User có thuộc tính Patient khác null
            _userRepositoryMock.Verify(
                r => r.AddAsync(
                    It.Is<User>(u => u.Patient != null && u.Patient.PatientId == 11), // Kiểm tra Patient được tạo và PatientId mới được tính toán (10 + 1)
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateUserAsync_ShouldHashPassword()
        {
            // Arrange
            const string rawPassword = "MySecret123!";
            var request = new CreateUserRequest
            {
                Phone = "0909123456",
                Password = rawPassword,
                RoleId = 1,
                FullName = "Test User"
            };

            _userRepositoryMock
                .Setup(r => r.GetByPhoneAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // Tạo một biến để lưu trữ đối tượng User được truyền vào AddAsync
            User capturedUser = null;

            // **THAY ĐỔI LỚN:** Sử dụng SetupCallback/Callback để lấy đối tượng User
            // Thay vì dùng It.Is<User>(...), ta dùng Callback
            _userRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Callback<User, CancellationToken>((user, token) => capturedUser = user) // Lưu đối tượng User
                .Returns(Task.CompletedTask);


            // Act
            await _userService.CreateUserAsync(request, CancellationToken.None);

            // Assert 
            // 1. Xác minh rằng AddAsync đã được gọi
            _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);

            // 2. **Kiểm tra BCrypt.Verify BÊN NGOÀI Verify của Moq**
            // Đây là nơi bạn kiểm tra hash password mà không gây lỗi Expression Tree
            Assert.NotNull(capturedUser);
            Assert.True(capturedUser.PasswordHash != rawPassword, "Password should be hashed.");

            // Dòng này không còn báo đỏ vì nó là một câu lệnh Assert độc lập!
            Assert.True(BCrypt.Net.BCrypt.Verify(rawPassword, capturedUser.PasswordHash), "Hashed password does not match the raw password.");

            // Kiểm tra các thuộc tính khác (nếu cần)
            Assert.Equal(request.Phone, capturedUser.Phone);
        }

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