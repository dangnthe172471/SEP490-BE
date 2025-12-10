using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.Repositories;

namespace SEP490_BE.Tests.Repositories;

public class MedicalRecordRepositoryTests
{
    private DiamondHealthContext NewCtx(string db)
    {
        var opt = new DbContextOptionsBuilder<DiamondHealthContext>()
            .UseInMemoryDatabase(db)
            .Options;
        return new DiamondHealthContext(opt);
    }

    [Fact]
    public async Task Create_And_GetByAppointment_Works()
    {
        using var ctx = NewCtx(nameof(Create_And_GetByAppointment_Works));
        ctx.Appointments.Add(new Appointment { AppointmentId = 1, PatientId = 1, DoctorId = 1, AppointmentDate = DateTime.Now });
        await ctx.SaveChangesAsync();

        var repo = new MedicalRecordRepository(ctx);
        var entity = new MedicalRecord { AppointmentId = 1, Diagnosis = "A" };
        var created = await repo.CreateAsync(entity, default);
        created.RecordId.Should().BeGreaterThan(0);

        var fetched = await repo.GetByAppointmentIdAsync(1, default);
        fetched.Should().NotBeNull();
        fetched!.AppointmentId.Should().Be(1);
    }

    [Fact]
    public async Task Update_ChangesFields()
    {
        using var ctx = NewCtx(nameof(Update_ChangesFields));
        ctx.Appointments.Add(new Appointment { AppointmentId = 2, PatientId = 1, DoctorId = 1, AppointmentDate = DateTime.Now });
        ctx.MedicalRecords.Add(new MedicalRecord { RecordId = 5, AppointmentId = 2, Diagnosis = "Old" });
        await ctx.SaveChangesAsync();

        var repo = new MedicalRecordRepository(ctx);
        var updated = await repo.UpdateAsync(5, "Note", "New", default);
        updated!.Diagnosis.Should().Be("New");
        updated.DoctorNotes.Should().Be("Note");
    }

    [Fact]
    public async Task GetAll_ReturnsAllRecords()
    {
        using var ctx = NewCtx(nameof(GetAll_ReturnsAllRecords));
        ctx.Appointments.AddRange(
            new Appointment { AppointmentId = 1, PatientId = 1, DoctorId = 1, AppointmentDate = DateTime.Now },
            new Appointment { AppointmentId = 2, PatientId = 1, DoctorId = 1, AppointmentDate = DateTime.Now }
        );
        ctx.MedicalRecords.AddRange(
            new MedicalRecord { RecordId = 1, AppointmentId = 1, Diagnosis = "Diagnosis 1" },
            new MedicalRecord { RecordId = 2, AppointmentId = 2, Diagnosis = "Diagnosis 2" }
        );
        await ctx.SaveChangesAsync();

        var repo = new MedicalRecordRepository(ctx);
        var result = await repo.GetAllAsync(default);
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_ReturnsEmpty_WhenNoRecords()
    {
        using var ctx = NewCtx(nameof(GetAll_ReturnsEmpty_WhenNoRecords));
        var repo = new MedicalRecordRepository(ctx);
        var result = await repo.GetAllAsync(default);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllByDoctor_ReturnsRecordsForDoctor()
    {
        using var ctx = NewCtx(nameof(GetAllByDoctor_ReturnsRecordsForDoctor));
        
        // Create users
        var user1 = new User { UserId = 1, Phone = "0909123456", FullName = "Doctor 1", RoleId = 3 };
        var user2 = new User { UserId = 2, Phone = "0909123457", FullName = "Doctor 2", RoleId = 3 };
        ctx.Users.AddRange(user1, user2);
        
        // Create doctors
        var doctor1 = new Doctor { DoctorId = 1, UserId = 1, Specialty = "Cardiology" };
        var doctor2 = new Doctor { DoctorId = 2, UserId = 2, Specialty = "Neurology" };
        ctx.Doctors.AddRange(doctor1, doctor2);
        
        // Create appointments
        ctx.Appointments.AddRange(
            new Appointment { AppointmentId = 1, PatientId = 1, DoctorId = 1, AppointmentDate = DateTime.Now },
            new Appointment { AppointmentId = 2, PatientId = 1, DoctorId = 1, AppointmentDate = DateTime.Now },
            new Appointment { AppointmentId = 3, PatientId = 1, DoctorId = 2, AppointmentDate = DateTime.Now }
        );
        
        // Create medical records
        ctx.MedicalRecords.AddRange(
            new MedicalRecord { RecordId = 1, AppointmentId = 1, Diagnosis = "Diagnosis 1" },
            new MedicalRecord { RecordId = 2, AppointmentId = 2, Diagnosis = "Diagnosis 2" },
            new MedicalRecord { RecordId = 3, AppointmentId = 3, Diagnosis = "Diagnosis 3" }
        );
        await ctx.SaveChangesAsync();

        var repo = new MedicalRecordRepository(ctx);
        var result = await repo.GetAllByDoctorAsync(1, default); // UserId = 1 (Doctor 1)
        result.Should().HaveCount(2); // Should return 2 records for Doctor 1
    }

    [Fact]
    public async Task GetById_ReturnsRecord_WhenExists()
    {
        using var ctx = NewCtx(nameof(GetById_ReturnsRecord_WhenExists));
        ctx.Appointments.Add(new Appointment { AppointmentId = 1, PatientId = 1, DoctorId = 1, AppointmentDate = DateTime.Now });
        ctx.MedicalRecords.Add(new MedicalRecord { RecordId = 10, AppointmentId = 1, Diagnosis = "Test Diagnosis" });
        await ctx.SaveChangesAsync();

        var repo = new MedicalRecordRepository(ctx);
        var result = await repo.GetByIdAsync(10, default);
        result.Should().NotBeNull();
        result!.RecordId.Should().Be(10);
        result.Diagnosis.Should().Be("Test Diagnosis");
    }

    [Fact]
    public async Task GetById_ReturnsNull_WhenNotExists()
    {
        using var ctx = NewCtx(nameof(GetById_ReturnsNull_WhenNotExists));
        var repo = new MedicalRecordRepository(ctx);
        var result = await repo.GetByIdAsync(999, default);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByAppointmentId_ReturnsRecord_WhenExists()
    {
        using var ctx = NewCtx(nameof(GetByAppointmentId_ReturnsRecord_WhenExists));
        ctx.Appointments.Add(new Appointment { AppointmentId = 5, PatientId = 1, DoctorId = 1, AppointmentDate = DateTime.Now });
        ctx.MedicalRecords.Add(new MedicalRecord { RecordId = 1, AppointmentId = 5, Diagnosis = "Test" });
        await ctx.SaveChangesAsync();

        var repo = new MedicalRecordRepository(ctx);
        var result = await repo.GetByAppointmentIdAsync(5, default);
        result.Should().NotBeNull();
        result!.AppointmentId.Should().Be(5);
    }

    [Fact]
    public async Task GetByAppointmentId_ReturnsNull_WhenNotExists()
    {
        using var ctx = NewCtx(nameof(GetByAppointmentId_ReturnsNull_WhenNotExists));
        var repo = new MedicalRecordRepository(ctx);
        var result = await repo.GetByAppointmentIdAsync(999, default);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Update_ReturnsNull_WhenRecordNotExists()
    {
        using var ctx = NewCtx(nameof(Update_ReturnsNull_WhenRecordNotExists));
        var repo = new MedicalRecordRepository(ctx);
        var result = await repo.UpdateAsync(999, "Note", "Diagnosis", default);
        result.Should().BeNull();
    }
}




