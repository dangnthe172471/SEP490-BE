using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.Tests.Controllers;

public class AuthRegisterTests
{
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IConfiguration> _config = new();
    private readonly Mock<SEP490_BE.DAL.IRepositories.IUserRepository> _userRepo = new();
    private readonly Mock<SEP490_BE.BLL.IServices.IResetTokenService> _resetSvc = new();
    private readonly Mock<SEP490_BE.BLL.IServices.IEmailService> _emailSvc = new();

    private AuthController CreateController()
    {
        return new AuthController(_userService.Object, _config.Object, _userRepo.Object, _resetSvc.Object, _emailSvc.Object);
    }

    [Fact()]
    public async Task Register_WithValidRequest_ReturnsOkWithUserId()
    {
        // Arrange - Test Case: Thông tin hợp lệ -> trả về userId
        var req = new RegisterRequest
        {
            Phone = "0912345678",
            Password = "123456",
            FullName = "John Doe",
            Email = "john@example.com",
            Dob = new DateOnly(1990, 1, 1),
            Gender = "Male",
            RoleId = 2
        };

        _userService
            .Setup(s => s.RegisterAsync(req.Phone, req.Password, req.FullName, req.Email, req.Dob, req.Gender, req.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(101);

        var controller = CreateController();

        // Act - Gọi API đăng ký
        var result = await controller.Register(req, default);

        // Assert - 200 OK và có userId
        var ok = Assert.IsType<OkObjectResult>(result);
        ok.StatusCode.Should().Be(200);
        ok.Value.Should().NotBeNull();
        _userService.VerifyAll();
    }

    [Fact()]
    public async Task Register_WhenPhoneExists_ThrowsInvalidOperationException()
    {
        // Arrange - Test Case: Số điện thoại đã tồn tại -> ném InvalidOperationException
        var req = new RegisterRequest
        {
            Phone = "0905123456",
            Password = "123456",
            FullName = "Existing User",
            RoleId = 2
        };

        _userService
            .Setup(s => s.RegisterAsync(req.Phone, req.Password, req.FullName, req.Email, req.Dob, req.Gender, req.RoleId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Phone already exists."));

        var controller = CreateController();

        // Act + Assert - API ném InvalidOperationException
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.Register(req, default));
        _userService.VerifyAll();
    }
}


