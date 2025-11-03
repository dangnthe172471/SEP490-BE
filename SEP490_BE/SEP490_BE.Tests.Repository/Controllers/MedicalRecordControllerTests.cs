using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.MedicalRecordDTO;
using SEP490_BE.DAL.Models;
using System.Threading;

namespace SEP490_BE.Tests.Controllers;

public class MedicalRecordControllerTests
{
    private readonly Mock<IMedicalRecordService> _svc = new(MockBehavior.Strict);

    private MedicalRecordController NewCtrl() => new(_svc.Object);

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _svc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MedicalRecord>());

        var res = await NewCtrl().GetAllAsync(default);
        var ok = Assert.IsType<OkObjectResult>(res.Result);
        ok.StatusCode.Should().Be(200);
        _svc.VerifyAll();
    }

    [Fact]
    public async Task GetById_NotFound()
    {
        _svc.Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalRecord?)null);

        var res = await NewCtrl().GetByIdAsync(99, default);
        var nf = Assert.IsType<NotFoundObjectResult>(res.Result);
        nf.StatusCode.Should().Be(404);
        _svc.VerifyAll();
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var dto = new CreateMedicalRecordDto { AppointmentId = 1, Diagnosis = "A", DoctorNotes = "B" };
        var created = new MedicalRecord { RecordId = 10, AppointmentId = 1, Diagnosis = "A", DoctorNotes = "B" };
        _svc.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var res = await NewCtrl().CreateAsync(dto, default);
        var cr = Assert.IsType<CreatedAtRouteResult>(res.Result);
        cr.RouteName.Should().Be("GetMedicalRecordById");
        cr.StatusCode.Should().Be(201);
        _svc.VerifyAll();
    }

    [Fact]
    public async Task Update_NotFound()
    {
        var dto = new UpdateMedicalRecordDto { Diagnosis = "X" };
        _svc.Setup(s => s.UpdateAsync(5, dto, It.IsAny<CancellationToken>())).ReturnsAsync((MedicalRecord?)null);

        var res = await NewCtrl().UpdateAsync(5, dto, default);
        var nf = Assert.IsType<NotFoundObjectResult>(res.Result);
        nf.StatusCode.Should().Be(404);
        _svc.VerifyAll();
    }

    [Fact]
    public async Task GetByAppointment_ReturnsOk()
    {
        _svc.Setup(s => s.GetByAppointmentIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MedicalRecord { RecordId = 1, AppointmentId = 1 });

        var res = await NewCtrl().GetByAppointmentIdAsync(1, default);
        var ok = Assert.IsType<OkObjectResult>(res.Result);
        ok.StatusCode.Should().Be(200);
        _svc.VerifyAll();
    }
}


