using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.TestReDTO;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories
{
    public class TestResultRepository : ITestResultRepository
    {
        private readonly DiamondHealthContext _db;
        public TestResultRepository(DiamondHealthContext db) => _db = db;

        public Task<TestResult?> GetEntityByIdAsync(int id, CancellationToken ct = default)
            => _db.TestResults
                .Include(tr => tr.Service)
                .FirstOrDefaultAsync(tr => tr.TestResultId == id, ct);

        public Task<List<TestResult>> GetEntitiesByRecordIdAsync(int recordId, CancellationToken ct = default)
            => _db.TestResults
                .Include(tr => tr.Service)
                .Where(tr => tr.RecordId == recordId)
                .OrderBy(tr => tr.ServiceId)
                .ToListAsync(ct);

        public async Task<TestResult> CreateAsync(TestResult entity, CancellationToken ct = default)
        {
            _db.TestResults.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task UpdateAsync(TestResult entity, CancellationToken ct = default)
        {
            _db.TestResults.Update(entity);
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _db.TestResults
                .FirstOrDefaultAsync(x => x.TestResultId == id, ct);

            if (entity != null)
            {
                _db.TestResults.Remove(entity);
                await _db.SaveChangesAsync(ct);
            }
        }

        public Task<bool> RecordExistsAsync(int recordId, CancellationToken ct = default)
            => _db.MedicalRecords.AnyAsync(r => r.RecordId == recordId, ct);

        public Task<bool> TestTypeExistsAsync(int testTypeId, CancellationToken ct = default)
            => _db.Services.AnyAsync(
                t => t.ServiceId == testTypeId && t.Category == "Test" && t.IsActive,
                ct);

        public Task<List<Service>> GetTestTypeEntitiesAsync(CancellationToken ct = default)
            => _db.Services
                .Where(s => s.IsActive && s.Category == "Test")
                .OrderBy(s => s.ServiceName)
                .ToListAsync(ct);

        public async Task<List<MedicalRecord>> GetWorklistEntitiesAsync(
            DateOnly? visitDate,
            string? patientName,
            CancellationToken ct = default)
        {
            var q = _db.MedicalRecords
                .OrderByDescending(r => r.CreatedAt)
                .Include(r => r.Appointment)!.ThenInclude(a => a.Patient)!.ThenInclude(p => p.User)
                .Include(r => r.TestResults)!.ThenInclude(tr => tr.Service)
                .AsQueryable();

            if (visitDate.HasValue)
            {
                var start = visitDate.Value.ToDateTime(TimeOnly.MinValue);
                var next = visitDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
                q = q.Where(r => r.Appointment.AppointmentDate >= start &&
                                 r.Appointment.AppointmentDate < next);
            }

            if (!string.IsNullOrWhiteSpace(patientName))
            {
                var like = $"%{patientName.Trim()}%";
                q = q.Where(r => EF.Functions.Like(r.Appointment.Patient.User.FullName, like));
            }

            return await q.ToListAsync(ct);
        }

        public async Task EnsureMedicalServiceForTestAsync(int recordId, int serviceId, CancellationToken ct = default)
        {
            var existed = await _db.MedicalServices
                .AnyAsync(ms => ms.RecordId == recordId && ms.ServiceId == serviceId, ct);

            if (existed) return;

            var service = await _db.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId, ct)
                ?? throw new KeyNotFoundException($"Dịch vụ (Service) với mã {serviceId} không tồn tại.");

            var price = service.Price ?? 0m;

            var medService = new MedicalService
            {
                RecordId = recordId,
                ServiceId = serviceId,
                Quantity = 1,
                UnitPrice = price,
                TotalPrice = price,
                Notes = "Dịch vụ xét nghiệm",
                CreatedAt = DateTime.UtcNow
            };

            _db.MedicalServices.Add(medService);
            await _db.SaveChangesAsync(ct);
        }
    }
}
