using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers;

public class UsersAdminCrudTests
{
    private readonly Mock<IUserService> _svc = new(MockBehavior.Strict);

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

    private UsersController NewCtrl()
    {
        var ctrl = new UsersController(_svc.Object);
        ctrl.ControllerContext = AdminContext();
        return ctrl;
    }

    [Fact]
    public async Task Admin_CreateUser_ReturnsCreated()
    {
        var req = new CreateUserRequest { Phone = "0909999999", Password = "123456", FullName = "New Admin User", RoleId = 2 };
        var dto = new UserDto { UserId = 200, FullName = req.FullName, Role = "Patient", Phone = req.Phone, IsActive = true };
        _svc.Setup(s => s.CreateUserAsync(req, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var res = await NewCtrl().CreateUser(req, default);
        var created = Assert.IsType<CreatedAtActionResult>(res.Result);
        created.StatusCode.Should().Be(201);
        _svc.VerifyAll();
    }

    [Fact]
    public async Task Admin_UpdateUser_ReturnsOk()
    {
        var req = new UpdateUserRequest { FullName = "Updated Name" };
        var dto = new UserDto { UserId = 10, FullName = "Updated Name" };
        _svc.Setup(s => s.UpdateUserAsync(10, req, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var res = await NewCtrl().UpdateUser(10, req, default);
        var ok = Assert.IsType<OkObjectResult>(res.Result);
        ok.StatusCode.Should().Be(200);
        _svc.VerifyAll();
    }

    [Fact]
    public async Task Admin_DeleteUser_NotFound()
    {
        _svc.Setup(s => s.DeleteUserAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var res = await NewCtrl().DeleteUser(999, default);
        var nf = Assert.IsType<NotFoundObjectResult>(res);
        nf.StatusCode.Should().Be(404);
        _svc.VerifyAll();
    }

    [Fact]
    public async Task Admin_DeleteUser_NoContent()
    {
        _svc.Setup(s => s.DeleteUserAsync(11, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var res = await NewCtrl().DeleteUser(11, default);
        Assert.IsType<NoContentResult>(res);
        _svc.VerifyAll();
    }

    [Fact]
    public async Task Admin_ToggleStatus_ReturnsOk()
    {
        _svc.Setup(s => s.ToggleUserStatusAsync(12, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var updated = new UserDto { UserId = 12, IsActive = false };
        _svc.Setup(s => s.GetByIdAsync(12, It.IsAny<CancellationToken>())).ReturnsAsync(updated);

        var res = await NewCtrl().ToggleUserStatus(12, default);
        var ok = Assert.IsType<OkObjectResult>(res.Result);
        ok.StatusCode.Should().Be(200);
        _svc.VerifyAll();
    }

}




