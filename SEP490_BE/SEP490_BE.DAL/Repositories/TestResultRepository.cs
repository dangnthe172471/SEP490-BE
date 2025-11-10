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
                .Include(tr => tr.TestType)
                .Where(tr => tr.TestResultId == id)
                .Select(tr => new ReadTestResultDto
                {
                    TestResultId = tr.TestResultId,
                    RecordId = tr.RecordId,
                    TestTypeId = tr.TestTypeId,
                    TestName = tr.TestType.TestName,
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
                .Include(tr => tr.TestType)
                .Where(tr => tr.RecordId == recordId)
                .OrderBy(tr => tr.TestTypeId)
                .Select(tr => new ReadTestResultDto
                {
                    TestResultId = tr.TestResultId,
                    RecordId = tr.RecordId,
                    TestTypeId = tr.TestTypeId,
                    TestName = tr.TestType.TestName,
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
            => _db.TestTypes.AnyAsync(t => t.TestTypeId == testTypeId, ct);

        public Task<TestResult?> GetEntityByIdAsync(int id, CancellationToken ct = default)
            => _db.TestResults.FirstOrDefaultAsync(x => x.TestResultId == id, ct);

        public async Task<PagedResult<TestWorklistItemDto>> GetWorklistAsync(
            DateOnly? visitDate,
            string? patientName,
            int pageNumber,
            int pageSize,
            RequiredState requiredState,
            CancellationToken ct = default)
        {
            // Lấy tất cả TestType
            var requiredTestTypeIds = await _db.TestTypes
                .AsNoTracking()
                .Select(t => t.TestTypeId)
                .ToListAsync(ct);

            if (requiredTestTypeIds.Count == 0)
                throw new InvalidOperationException("Không tìm thấy TestType nào trong DB. Hãy seed dữ liệu TestType.");

            var q = _db.MedicalRecords
                .AsNoTracking()
                .Include(r => r.Appointment)!.ThenInclude(a => a.Patient)!.ThenInclude(p => p.User)
                .Include(r => r.TestResults)!.ThenInclude(tr => tr.TestType)
                .AsQueryable();

            // ⬅ CHỈ lọc ngày khi có visitDate
            if (visitDate.HasValue)
            {
                var start = visitDate.Value.ToDateTime(TimeOnly.MinValue);
                var next = visitDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
                q = q.Where(r => r.Appointment.AppointmentDate >= start &&
                                 r.Appointment.AppointmentDate < next);
            }

            // lọc tên (bỏ qua placeholder)
            if (!string.IsNullOrWhiteSpace(patientName) &&
                !string.Equals(patientName, "patientName", StringComparison.OrdinalIgnoreCase))
            {
                var like = $"%{patientName.Trim()}%";
                q = q.Where(r => EF.Functions.Like(r.Appointment.Patient.User.FullName, like));
            }

            // lọc trạng thái
            if (requiredState != RequiredState.All)
            {
                if (requiredState == RequiredState.Missing)
                {
                    q = q.Where(r => !requiredTestTypeIds
                        .All(reqId => r.TestResults.Any(tr => tr.TestTypeId == reqId)));
                }
                else // Complete
                {
                    q = q.Where(r => requiredTestTypeIds
                        .All(reqId => r.TestResults.Any(tr => tr.TestTypeId == reqId)));
                }
            }

            var total = await q.CountAsync(ct);

            var items = await q
                .OrderBy(r => r.Appointment.AppointmentDate)
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
                        .All(reqId => r.TestResults.Any(tr => tr.TestTypeId == reqId)),
                    Results = r.TestResults.Select(tr => new ReadTestResultDto
                    {
                        TestResultId = tr.TestResultId,
                        RecordId = tr.RecordId,
                        TestTypeId = tr.TestTypeId,
                        TestName = tr.TestType.TestName,
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

        public Task<List<TestType>> GetAllTestTypesAsync(CancellationToken ct = default)
            => _db.TestTypes.AsNoTracking().OrderBy(t => t.TestName).ToListAsync(ct);
    }
}



