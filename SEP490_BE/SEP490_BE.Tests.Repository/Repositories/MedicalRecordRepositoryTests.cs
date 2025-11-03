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
}


