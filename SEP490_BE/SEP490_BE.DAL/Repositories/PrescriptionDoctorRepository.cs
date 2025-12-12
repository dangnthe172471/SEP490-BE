using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.PrescriptionDoctorDTO;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories
{
    public class PrescriptionDoctorRepository : IPrescriptionDoctorRepository
    {
        private readonly DiamondHealthContext _db;
        public PrescriptionDoctorRepository(DiamondHealthContext db) => _db = db;

        public Task<Doctor?> GetDoctorByUserIdAsync(int userId, CancellationToken ct)
            => _db.Doctors
                  .Include(d => d.User)
                  .AsNoTracking()
                  .FirstOrDefaultAsync(d => d.UserId == userId, ct);

        public Task<MedicalRecord?> GetRecordWithAppointmentAsync(int recordId, CancellationToken ct)
            => _db.MedicalRecords
                  .Include(r => r.Appointment)
                      .ThenInclude(a => a.Patient)
                          .ThenInclude(p => p.User)
                  .Include(r => r.Appointment)
                      .ThenInclude(a => a.Doctor)
                          .ThenInclude(d => d.User)
                  .AsNoTracking()
                  .FirstOrDefaultAsync(r => r.RecordId == recordId, ct);

        public async Task<Dictionary<int, Medicine>> GetMedicinesByIdsAsync(
            IEnumerable<int> ids,
            CancellationToken ct)
        {
            var arr = ids.Distinct().ToArray();

            var meds = await _db.Medicines
                .Include(m => m.Provider).ThenInclude(p => p.User)
                .AsNoTracking()
                .Where(m => arr.Contains(m.MedicineId))
                .ToListAsync(ct);

            return meds.ToDictionary(m => m.MedicineId, m => m);
        }

        public async Task<Dictionary<int, MedicineVersion>> GetLatestMedicineVersionsByMedicineIdsAsync(
            IEnumerable<int> ids,
            CancellationToken ct)
        {
            var arr = ids.Distinct().ToArray();

            var latest = await _db.MedicineVersions
                .Where(v => arr.Contains(v.MedicineId))
                .GroupBy(v => v.MedicineId)
                .Select(g => g.OrderByDescending(v => v.MedicineVersionId).First())
                .AsNoTracking()
                .ToListAsync(ct);

            return latest.ToDictionary(v => v.MedicineId, v => v);
        }

        public async Task<Prescription> CreatePrescriptionAsync(
            Prescription header,
            IEnumerable<PrescriptionDetail> details,
            CancellationToken ct)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            _db.Prescriptions.Add(header);
            await _db.SaveChangesAsync(ct);

            foreach (var d in details)
                d.PrescriptionId = header.PrescriptionId;

            _db.PrescriptionDetails.AddRange(details);
            await _db.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);
            return header;
        }

        public Task<Prescription?> GetPrescriptionGraphAsync(int prescriptionId, CancellationToken ct)
            => _db.Prescriptions
                  .Include(p => p.Record)
                      .ThenInclude(r => r.Appointment)
                          .ThenInclude(a => a.Patient)
                              .ThenInclude(p => p.User)
                  .Include(p => p.Record)
                      .ThenInclude(r => r.Appointment)
                          .ThenInclude(a => a.Doctor)
                              .ThenInclude(d => d.User)
                  .Include(p => p.PrescriptionDetails)
                      .ThenInclude(d => d.MedicineVersion)
                  .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId, ct);

        public async Task<(List<MedicalRecord> Items, int TotalCount)> GetRecordsForDoctorAsync(
            int doctorId,
            DateOnly? visitDateFrom,
            DateOnly? visitDateTo,
            string? patientNameSearch,
            int pageNumber,
            int pageSize,
            CancellationToken ct)
        {
            var q = _db.MedicalRecords
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Patient)
                        .ThenInclude(p => p.User)
                .Where(r => r.Appointment.DoctorId == doctorId)
                .AsQueryable();

            if (visitDateFrom.HasValue)
            {
                var from = visitDateFrom.Value.ToDateTime(TimeOnly.MinValue);
                q = q.Where(r => r.Appointment.AppointmentDate >= from);
            }

            if (visitDateTo.HasValue)
            {
                var toExclusive = visitDateTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
                q = q.Where(r => r.Appointment.AppointmentDate < toExclusive);
            }

            if (!string.IsNullOrWhiteSpace(patientNameSearch))
            {
                var s = patientNameSearch.Trim();
                q = q.Where(r => r.Appointment.Patient.User.FullName != null &&
                                 EF.Functions.Like(r.Appointment.Patient.User.FullName, $"%{s}%"));
            }

            var total = await q.CountAsync(ct);

            var items = await q
                .OrderByDescending(r => r.Appointment.AppointmentDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<Dictionary<int, int?>> GetLatestPrescriptionIdsByRecordIdsAsync(
            IEnumerable<int> recordIds,
            CancellationToken ct)
        {
            var ids = recordIds.Distinct().ToArray();

            var latest = await _db.Prescriptions
                .Where(p => ids.Contains(p.RecordId))
                .GroupBy(p => p.RecordId)
                .Select(g => new
                {
                    RecordId = g.Key,
                    LatestPrescriptionId = (int?)g.Max(p => p.PrescriptionId)
                })
                .AsNoTracking()
                .ToListAsync(ct);

            return latest.ToDictionary(x => x.RecordId, x => x.LatestPrescriptionId);
        }
    }
}
