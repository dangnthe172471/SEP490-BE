using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers;

public class UsersReceptionistPatientTests
{
    private readonly Mock<IUserService> _svc = new(MockBehavior.Strict);

    private static ControllerContext ReceptionistContext()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "9"),
            new(ClaimTypes.Role, "Receptionist")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        return new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
    }

    private UsersController NewCtrl()
    {
        var ctrl = new UsersController(_svc.Object);
        ctrl.ControllerContext = ReceptionistContext();
        return ctrl;
    }

    [Fact]
    public async Task CreatePatient_ByReceptionist_ReturnsCreated()
    {
        var req = new CreateUserRequest
        {
            Phone = "0912345678",
            Password = "123456",
            FullName = "Patient A",
            RoleId = 2,
            Gender = "Male"
        };
        var dto = new UserDto { UserId = 100, FullName = "Patient A", Role = "Patient", Phone = "0912345678", IsActive = true };

        _svc.Setup(s => s.CreateUserAsync(req, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var res = await NewCtrl().CreateUser(req, default);
        var created = Assert.IsType<CreatedAtActionResult>(res.Result);
        created.StatusCode.Should().Be(201);
        _svc.VerifyAll();
    }

    [Fact]
    public async Task GetAllPatients_ReturnsOk()
    {
        _svc.Setup(s => s.GetAllPatientsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserDto>());

        var res = await NewCtrl().GetAllPatients(default);
        var ok = Assert.IsType<OkObjectResult>(res.Result);
        ok.StatusCode.Should().Be(200);
        _svc.VerifyAll();
    }

}


