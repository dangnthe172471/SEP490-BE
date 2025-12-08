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

    [Fact]
    public async Task GetAll_ReturnsAllRecords()
    {
        var records = new List<MedicalRecord>
        {
            new MedicalRecord { RecordId = 1, AppointmentId = 1 },
            new MedicalRecord { RecordId = 2, AppointmentId = 2 }
        };
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        var result = await NewSvc().GetAllAsync(default);
        result.Should().HaveCount(2);
        _repo.VerifyAll();
    }

    [Fact]
    public async Task GetAllByDoctor_ReturnsRecordsForDoctor()
    {
        var records = new List<MedicalRecord>
        {
            new MedicalRecord { RecordId = 1, AppointmentId = 1, Diagnosis = "Test" }
        };
        _repo.Setup(r => r.GetAllByDoctorAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        var result = await NewSvc().GetAllByDoctorAsync(1, default);
        result.Should().HaveCount(1);
        _repo.VerifyAll();
    }

    [Fact]
    public async Task GetById_ReturnsRecord_WhenExists()
    {
        var record = new MedicalRecord { RecordId = 1, AppointmentId = 1, Diagnosis = "Test" };
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        var result = await NewSvc().GetByIdAsync(1, default);
        result.Should().NotBeNull();
        result!.RecordId.Should().Be(1);
        _repo.VerifyAll();
    }

    [Fact]
    public async Task GetById_ReturnsNull_WhenNotExists()
    {
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalRecord?)null);

        var result = await NewSvc().GetByIdAsync(999, default);
        result.Should().BeNull();
        _repo.VerifyAll();
    }

    [Fact]
    public async Task Create_CreatesNew_WhenNotExists()
    {
        var dto = new CreateMedicalRecordDto { AppointmentId = 1, Diagnosis = "New", DoctorNotes = "Notes" };
        _repo.Setup(r => r.GetByAppointmentIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalRecord?)null);
        
        var newRecord = new MedicalRecord { RecordId = 10, AppointmentId = 1, Diagnosis = "New", DoctorNotes = "Notes" };
        _repo.Setup(r => r.CreateAsync(It.IsAny<MedicalRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newRecord);

        var result = await NewSvc().CreateAsync(dto, default);
        result.RecordId.Should().Be(10);
        _repo.VerifyAll();
    }

    [Fact]
    public async Task GetByAppointmentId_ReturnsRecord_WhenExists()
    {
        var record = new MedicalRecord { RecordId = 1, AppointmentId = 5, Diagnosis = "Test" };
        _repo.Setup(r => r.GetByAppointmentIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        var result = await NewSvc().GetByAppointmentIdAsync(5, default);
        result.Should().NotBeNull();
        result!.AppointmentId.Should().Be(5);
        _repo.VerifyAll();
    }

    [Fact]
    public async Task GetByAppointmentId_ReturnsNull_WhenNotExists()
    {
        _repo.Setup(r => r.GetByAppointmentIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalRecord?)null);

        var result = await NewSvc().GetByAppointmentIdAsync(999, default);
        result.Should().BeNull();
        _repo.VerifyAll();
    }
}


