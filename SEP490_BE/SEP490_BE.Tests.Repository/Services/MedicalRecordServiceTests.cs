using FluentAssertions;
using Moq;
using SEP490_BE.BLL.Services;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.MedicalRecordDTO;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using System.Threading;

namespace SEP490_BE.Tests.Services;

public class MedicalRecordServiceTests
{
    private readonly Mock<IMedicalRecordRepository> _repo = new(MockBehavior.Strict);

    private MedicalRecordService NewSvc() => new(_repo.Object);

    [Fact]
    public async Task Create_ReturnsExisting_WhenAlreadyExists()
    {
        var dto = new CreateMedicalRecordDto { AppointmentId = 1 };
        var existing = new MedicalRecord { RecordId = 3, AppointmentId = 1 };
        _repo.Setup(r => r.GetByAppointmentIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await NewSvc().CreateAsync(dto, default);
        result.RecordId.Should().Be(3);
        _repo.VerifyAll();
    }

    [Fact]
    public async Task Update_PassesThrough()
    {
        var dto = new UpdateMedicalRecordDto { Diagnosis = "X" };
        _repo.Setup(r => r.UpdateAsync(5, dto.DoctorNotes, dto.Diagnosis, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MedicalRecord { RecordId = 5, Diagnosis = "X" });

        var res = await NewSvc().UpdateAsync(5, dto, default);
        res!.Diagnosis.Should().Be("X");
        _repo.VerifyAll();
    }
}


