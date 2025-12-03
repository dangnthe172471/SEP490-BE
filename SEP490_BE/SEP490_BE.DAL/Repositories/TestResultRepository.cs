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

        public async Task<ReadTestResultDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.TestResults
                .AsNoTracking()
                .Include(tr => tr.Service)
                .Where(tr => tr.TestResultId == id)
                .Select(tr => new ReadTestResultDto
                {
                    TestResultId = tr.TestResultId,
                    RecordId = tr.RecordId,
                    TestTypeId = tr.ServiceId,
                    TestName = tr.Service.ServiceName,
                    ResultValue = tr.ResultValue,
                    Unit = tr.Unit,
                    Attachment = tr.Attachment,
                    ResultDate = tr.ResultDate,
                    Notes = tr.Notes
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<List<ReadTestResultDto>> GetByRecordIdAsync(int recordId, CancellationToken ct = default)
        {
            return await _db.TestResults
                .AsNoTracking()
                .Include(tr => tr.Service)
                .Where(tr => tr.RecordId == recordId)
                .OrderBy(tr => tr.ServiceId)
                .Select(tr => new ReadTestResultDto
                {
                    TestResultId = tr.TestResultId,
                    RecordId = tr.RecordId,
                    TestTypeId = tr.ServiceId,
                    TestName = tr.Service.ServiceName,
                    ResultValue = tr.ResultValue,
                    Unit = tr.Unit,
                    Attachment = tr.Attachment,
                    ResultDate = tr.ResultDate,
                    Notes = tr.Notes
                })
                .ToListAsync(ct);
        }

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
            var entity = await _db.TestResults.FirstOrDefaultAsync(x => x.TestResultId == id, ct)
                ?? throw new KeyNotFoundException($"TestResult {id} not found");
            _db.TestResults.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }

        public Task<bool> RecordExistsAsync(int recordId, CancellationToken ct = default)
            => _db.MedicalRecords.AnyAsync(r => r.RecordId == recordId, ct);

        public Task<bool> TestTypeExistsAsync(int testTypeId, CancellationToken ct = default)
            => _db.Services.AnyAsync(
                t => t.ServiceId == testTypeId && t.Category == "Test" && t.IsActive,
                ct);

        public Task<TestResult?> GetEntityByIdAsync(int id, CancellationToken ct = default)
            => _db.TestResults.FirstOrDefaultAsync(x => x.TestResultId == id, ct);

        public async Task<List<TestTypeLite>> GetTestTypesAsync(CancellationToken ct = default)
        {
            return await _db.Services
                .AsNoTracking()
                .Where(s => s.IsActive && s.Category == "Test")
                .OrderBy(s => s.ServiceName)
                .Select(s => new TestTypeLite
                {
                    TestTypeId = s.ServiceId,
                    TestName = s.ServiceName,
                    Price = s.Price
                })
                .ToListAsync(ct);
        }

        public async Task EnsureMedicalServiceForTestAsync(int recordId, int serviceId, CancellationToken ct = default)
        {
            var existed = await _db.MedicalServices
                .AnyAsync(ms => ms.RecordId == recordId && ms.ServiceId == serviceId, ct);

            if (existed) return;

            var service = await _db.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId, ct)
                ?? throw new KeyNotFoundException($"Service {serviceId} không tồn tại");

            var unitPrice = service.Price ?? 0m;

            var medService = new MedicalService
            {
                RecordId = recordId,
                ServiceId = serviceId,
                Quantity = 1,
                UnitPrice = unitPrice,
                TotalPrice = unitPrice,
                Notes = "Dịch vụ xét nghiệm",
                CreatedAt = DateTime.UtcNow
            };

            _db.MedicalServices.Add(medService);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<PagedResult<TestWorklistItemDto>> GetWorklistAsync(
            DateOnly? visitDate,
            string? patientName,
            int pageNumber,
            int pageSize,
            RequiredState requiredState,
            CancellationToken ct = default)
        {
            var requiredTestTypeIds = await _db.Services
                .AsNoTracking()
                .Where(s => s.IsActive && s.Category == "Test")
                .Select(t => t.ServiceId)
                .ToListAsync(ct);

            if (requiredTestTypeIds.Count == 0)
                throw new InvalidOperationException("Không tìm thấy loại xét nghiệm nào trong DB. Hãy seed dữ liệu Service(Category='Test').");

            var q = _db.MedicalRecords
                .AsNoTracking()
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

            if (!string.IsNullOrWhiteSpace(patientName) &&
                !string.Equals(patientName, "patientName", StringComparison.OrdinalIgnoreCase))
            {
                var like = $"%{patientName.Trim()}%";
                q = q.Where(r => EF.Functions.Like(r.Appointment.Patient.User.FullName, like));
            }

            if (requiredState != RequiredState.All)
            {
                if (requiredState == RequiredState.Missing)
                {
                    q = q.Where(r => !requiredTestTypeIds
                        .All(reqId => r.TestResults.Any(tr => tr.ServiceId == reqId)));
                }
                else
                {
                    q = q.Where(r => requiredTestTypeIds
                        .All(reqId => r.TestResults.Any(tr => tr.ServiceId == reqId)));
                }
            }

            var total = await q.CountAsync(ct);

            var items = await q
                .OrderByDescending(r => r.Appointment.AppointmentDate)
                .ThenByDescending(r => r.RecordId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new TestWorklistItemDto
                {
                    RecordId = r.RecordId,
                    AppointmentId = r.AppointmentId,
                    AppointmentDate = r.Appointment.AppointmentDate,
                    PatientId = r.Appointment.Patient.PatientId,
                    PatientName = r.Appointment.Patient.User.FullName,
                    HasAllRequiredResults = requiredTestTypeIds
                        .All(reqId => r.TestResults.Any(tr => tr.ServiceId == reqId)),
                    Results = r.TestResults.Select(tr => new ReadTestResultDto
                    {
                        TestResultId = tr.TestResultId,
                        RecordId = tr.RecordId,
                        TestTypeId = tr.ServiceId,
                        TestName = tr.Service.ServiceName,
                        ResultValue = tr.ResultValue,
                        Unit = tr.Unit,
                        Attachment = tr.Attachment,
                        ResultDate = tr.ResultDate,
                        Notes = tr.Notes
                    }).ToList()
                })
                .ToListAsync(ct);

            return new PagedResult<TestWorklistItemDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total,
                Items = items
            };
        }
    }
}
