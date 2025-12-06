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
        // Arrange - Test Case UTCID02: Không có claim phone
        var res = await NewCtrl(null).UpdateBasicInfo(new UpdateBasicInfoRequest(), default);
        // Assert - 401 Unauthorized
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(res);
        unauthorized.StatusCode.Should().Be(401);
    }

    [Fact()]
    public async Task UpdateBasicInfo_UserNotFound_ReturnsNotFound()
    {
        // Arrange - Test Case UTCID03: Không tìm thấy người dùng theo phone
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
        // Arrange - Test Case UTCID01: Có user và cập nhật thành công
        var existing = new UserDto { UserId = 10, Phone = "0905111222", FullName = "A" };
        var updated = new UserDto { UserId = 10, Phone = "0905111222", FullName = "Nguyễn Văn A", Email = "nguyena@gmail.com" };

        _svc.Setup(s => s.GetUserByPhoneAsync(existing.Phone!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _svc.Setup(s => s.UpdateBasicInfoAsync(existing.UserId, It.IsAny<UpdateBasicInfoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        // Act - Gọi API cập nhật basic-info
        var request = new UpdateBasicInfoRequest 
        { 
            FullName = "Nguyễn Văn A", 
            Email = "nguyena@gmail.com",
            Phone = "0905111222",
            Dob = "1990-01-01",
            Gender = "Male"
        };
        var res = await NewCtrl(existing.Phone).UpdateBasicInfo(request, default);
        
        // Assert - 200 OK kèm user và message
        var ok = Assert.IsType<OkObjectResult>(res);
        ok.StatusCode.Should().Be(200);
        ok.Value.Should().NotBeNull();
        
        var valueType = ok.Value!.GetType();
        var messageProperty = valueType.GetProperty("message");
        var messageValue = messageProperty?.GetValue(ok.Value)?.ToString();
        messageValue.Should().Be("Cập nhật thông tin cơ bản thành công");
        
        _svc.VerifyAll();
    }

    [Fact()]
    public async Task UpdateBasicInfo_UpdateReturnsNull_ReturnsNotFound()
    {
        // Arrange - Test Case UTCID04: User exists but UpdateBasicInfoAsync returns null
        var existing = new UserDto { UserId = 10, Phone = "0905111222", FullName = "A" };

        _svc.Setup(s => s.GetUserByPhoneAsync(existing.Phone!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _svc.Setup(s => s.UpdateBasicInfoAsync(existing.UserId, It.IsAny<UpdateBasicInfoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act - Gọi API cập nhật basic-info
        var res = await NewCtrl(existing.Phone).UpdateBasicInfo(new UpdateBasicInfoRequest { FullName = "B" }, default);
        
        // Assert - 404 NotFound
        var nf = Assert.IsType<NotFoundObjectResult>(res);
        nf.StatusCode.Should().Be(404);
        _svc.VerifyAll();
    }

    [Fact()]
    public async Task UpdateBasicInfo_ExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange - Test Case UTCID05: Service throws exception
        var existing = new UserDto { UserId = 10, Phone = "0905111222", FullName = "A" };

        _svc.Setup(s => s.GetUserByPhoneAsync(existing.Phone!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _svc.Setup(s => s.UpdateBasicInfoAsync(existing.UserId, It.IsAny<UpdateBasicInfoRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act - Gọi API cập nhật basic-info
        var res = await NewCtrl(existing.Phone).UpdateBasicInfo(new UpdateBasicInfoRequest { FullName = "B" }, default);
        
        // Assert - 500 InternalServerError
        var statusCode = Assert.IsType<ObjectResult>(res);
        statusCode.StatusCode.Should().Be(500);
        statusCode.Value.Should().NotBeNull();
        _svc.VerifyAll();
    }

    [Fact()]
    public async Task UpdateMedicalInfo_WithoutPhoneClaim_ReturnsUnauthorized()
    {
        // Arrange - Test Case UTCID07: Không có claim phone
        var res = await NewCtrl(null).UpdateMedicalInfo(new UpdateMedicalInfoRequest(), default);
        // Assert - 401 Unauthorized
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(res);
        unauthorized.StatusCode.Should().Be(401);
    }

    [Fact()]
    public async Task UpdateMedicalInfo_UserNotFound_ReturnsNotFound()
    {
        // Arrange - Test Case UTCID08: Không tìm thấy người dùng theo phone
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
        // Arrange - Test Case UTCID06: Có user và cập nhật medical-info thành công
        var existing = new UserDto { UserId = 12, Phone = "0905333444", FullName = "A" };
        var updated = new UserDto { UserId = 12, Phone = "0905333444", FullName = "A", Allergies = "Penicillin, Aspirin", MedicalHistory = "Tiểu đường" };

        _svc.Setup(s => s.GetUserByPhoneAsync(existing.Phone!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _svc.Setup(s => s.UpdateMedicalInfoAsync(existing.UserId, It.IsAny<UpdateMedicalInfoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        // Act - Gọi API cập nhật medical-info
        var request = new UpdateMedicalInfoRequest 
        { 
            Allergies = "Penicillin, Aspirin",
            MedicalHistory = "Tiểu đường"
        };
        var res = await NewCtrl(existing.Phone).UpdateMedicalInfo(request, default);
        
        // Assert - 200 OK kèm user và message
        var ok = Assert.IsType<OkObjectResult>(res);
        ok.StatusCode.Should().Be(200);
        ok.Value.Should().NotBeNull();
        
        var valueType = ok.Value!.GetType();
        var messageProperty = valueType.GetProperty("message");
        var messageValue = messageProperty?.GetValue(ok.Value)?.ToString();
        messageValue.Should().Be("Cập nhật thông tin y tế thành công");
        
        _svc.VerifyAll();
    }

    [Fact()]
    public async Task UpdateMedicalInfo_UpdateReturnsNull_ReturnsNotFound()
    {
        // Arrange - Test Case UTCID09: User exists but UpdateMedicalInfoAsync returns null (user not Patient)
        var existing = new UserDto { UserId = 12, Phone = "0905333444", FullName = "A" };

        _svc.Setup(s => s.GetUserByPhoneAsync(existing.Phone!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _svc.Setup(s => s.UpdateMedicalInfoAsync(existing.UserId, It.IsAny<UpdateMedicalInfoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act - Gọi API cập nhật medical-info
        var res = await NewCtrl(existing.Phone).UpdateMedicalInfo(new UpdateMedicalInfoRequest { Allergies = "Penicillin" }, default);
        
        // Assert - 404 NotFound với message phù hợp
        var nf = Assert.IsType<NotFoundObjectResult>(res);
        nf.StatusCode.Should().Be(404);
        nf.Value.Should().NotBeNull();
        _svc.VerifyAll();
    }

    [Fact()]
    public async Task UpdateMedicalInfo_ExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange - Test Case UTCID10: Service throws exception
        var existing = new UserDto { UserId = 12, Phone = "0905333444", FullName = "A" };

        _svc.Setup(s => s.GetUserByPhoneAsync(existing.Phone!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _svc.Setup(s => s.UpdateMedicalInfoAsync(existing.UserId, It.IsAny<UpdateMedicalInfoRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act - Gọi API cập nhật medical-info
        var res = await NewCtrl(existing.Phone).UpdateMedicalInfo(new UpdateMedicalInfoRequest { Allergies = "Penicillin" }, default);
        
        // Assert - 500 InternalServerError
        var statusCode = Assert.IsType<ObjectResult>(res);
        statusCode.StatusCode.Should().Be(500);
        statusCode.Value.Should().NotBeNull();
        _svc.VerifyAll();
    }
}


