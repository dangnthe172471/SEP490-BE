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
    }
}