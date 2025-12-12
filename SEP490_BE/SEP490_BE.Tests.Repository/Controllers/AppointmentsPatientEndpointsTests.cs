using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers.ManageReceptionist.ManageAppointment;
using SEP490_BE.BLL.IServices.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.IRepositories.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.DTOs.ManageReceptionist.ManageAppointment;

namespace SEP490_BE.Tests.Controllers;

public class AppointmentsPatientEndpointsTests
{
    private readonly Mock<IAppointmentService> _svc = new(MockBehavior.Strict);

    private AppointmentsController NewCtrl() => new(_svc.Object, Mock.Of<IAppointmentRepository>());

    [Fact]
    public async Task GetPatientById_Found()
    {
        var dto = new PatientInfoDto { PatientId = 1, FullName = "A" };
        _svc.Setup(s => s.GetPatientByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var res = await NewCtrl().GetPatientById(1, default);
        var ok = Assert.IsType<OkObjectResult>(res.Result);
        ok.StatusCode.Should().Be(200);
        _svc.VerifyAll();
    }

    [Fact]
    public async Task GetPatientByUserId_NotFound()
    {
        _svc.Setup(s => s.GetPatientInfoByUserIdAsync(9, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PatientInfoDto?)null);
        var res = await NewCtrl().GetPatientByUserId(9, default);
        var nf = Assert.IsType<NotFoundObjectResult>(res.Result);
        nf.StatusCode.Should().Be(404);
        _svc.VerifyAll();
    }
}

