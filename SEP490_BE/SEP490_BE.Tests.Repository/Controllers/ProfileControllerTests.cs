using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers;

public class ProfileControllerTests
{
    private readonly Mock<IUserService> _svc = new(MockBehavior.Strict);

    private static ControllerContext ContextWithPhone(string? phone)
    {
        var claims = new List<Claim>();
        if (!string.IsNullOrEmpty(phone))
        {
            claims.Add(new Claim(ClaimTypes.Name, phone));
        }
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        return new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
    }

    private ProfileController NewCtrl(string? phoneClaim)
    {
        var ctrl = new ProfileController(_svc.Object);
        ctrl.ControllerContext = ContextWithPhone(phoneClaim);
        return ctrl;
    }

    [Fact()]
    public async Task UpdateBasicInfo_WithoutPhoneClaim_ReturnsUnauthorized()
    {
        // Arrange - Test Case: Không có claim phone
        var res = await NewCtrl(null).UpdateBasicInfo(new UpdateBasicInfoRequest(), default);
        // Assert - 401 Unauthorized
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(res);
        unauthorized.StatusCode.Should().Be(401);
    }

    [Fact()]
    public async Task UpdateBasicInfo_UserNotFound_ReturnsNotFound()
    {
        // Arrange - Test Case: Không tìm thấy người dùng theo phone
        _svc.Setup(s => s.GetUserByPhoneAsync("0905000000", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act - Gọi API cập nhật basic-info
        var res = await NewCtrl("0905000000").UpdateBasicInfo(new UpdateBasicInfoRequest(), default);
        // Assert - 404 NotFound
        var nf = Assert.IsType<NotFoundObjectResult>(res);
        nf.StatusCode.Should().Be(404);
        _svc.VerifyAll();
    }

    [Fact()]
    public async Task UpdateBasicInfo_Success_ReturnsOkWithUser()
    {
        // Arrange - Test Case: Có user và cập nhật thành công
        var existing = new UserDto { UserId = 10, Phone = "0905111222", FullName = "A" };
        var updated = new UserDto { UserId = 10, Phone = "0905111222", FullName = "B" };

        _svc.Setup(s => s.GetUserByPhoneAsync(existing.Phone!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _svc.Setup(s => s.UpdateBasicInfoAsync(existing.UserId, It.IsAny<UpdateBasicInfoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        // Act - Gọi API cập nhật basic-info
        var res = await NewCtrl(existing.Phone).UpdateBasicInfo(new UpdateBasicInfoRequest { FullName = "B" }, default);
        // Assert - 200 OK kèm user
        var ok = Assert.IsType<OkObjectResult>(res);
        ok.StatusCode.Should().Be(200);
        ok.Value.Should().NotBeNull();
        _svc.VerifyAll();
    }

    [Fact()]
    public async Task UpdateMedicalInfo_WithoutPhoneClaim_ReturnsUnauthorized()
    {
        // Arrange - Test Case: Không có claim phone
        var res = await NewCtrl(null).UpdateMedicalInfo(new UpdateMedicalInfoRequest(), default);
        // Assert - 401 Unauthorized
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(res);
        unauthorized.StatusCode.Should().Be(401);
    }

    [Fact()]
    public async Task UpdateMedicalInfo_UserNotFound_ReturnsNotFound()
    {
        // Arrange - Test Case: Không tìm thấy người dùng theo phone
        _svc.Setup(s => s.GetUserByPhoneAsync("0905777888", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act - Gọi API cập nhật medical-info
        var res = await NewCtrl("0905777888").UpdateMedicalInfo(new UpdateMedicalInfoRequest(), default);
        // Assert - 404 NotFound
        var nf = Assert.IsType<NotFoundObjectResult>(res);
        nf.StatusCode.Should().Be(404);
        _svc.VerifyAll();
    }

    [Fact()]
    public async Task UpdateMedicalInfo_Success_ReturnsOkWithUser()
    {
        // Arrange - Test Case: Có user và cập nhật medical-info thành công
        var existing = new UserDto { UserId = 12, Phone = "0905333444", FullName = "A" };
        var updated = new UserDto { UserId = 12, Phone = "0905333444", FullName = "A", Allergies = "Penicillin" };

        _svc.Setup(s => s.GetUserByPhoneAsync(existing.Phone!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _svc.Setup(s => s.UpdateMedicalInfoAsync(existing.UserId, It.IsAny<UpdateMedicalInfoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        // Act - Gọi API cập nhật medical-info
        var res = await NewCtrl(existing.Phone).UpdateMedicalInfo(new UpdateMedicalInfoRequest { Allergies = "Penicillin" }, default);
        // Assert - 200 OK kèm user
        var ok = Assert.IsType<OkObjectResult>(res);
        ok.StatusCode.Should().Be(200);
        ok.Value.Should().NotBeNull();
        _svc.VerifyAll();
    }
}


