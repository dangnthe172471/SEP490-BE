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

        public async Task<PagedResult<RecordListItemDto>> GetRecordsForDoctorAsync(
            int userIdFromToken,
            DateOnly? visitDateFrom,
            DateOnly? visitDateTo,
            string? patientNameSearch,
            int pageNumber,
            int pageSize,
            CancellationToken ct)
        {
            var doctor = await _db.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userIdFromToken, ct)
                ?? throw new InvalidOperationException("Bác sĩ không tồn tại.");

            var q = _db.MedicalRecords
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Patient).ThenInclude(p => p.User)
                .Where(r => r.Appointment.DoctorId == doctor.DoctorId)
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
            var presQ = _db.Prescriptions.AsQueryable();

            var items = await q
                .OrderByDescending(r => r.Appointment.AppointmentDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RecordListItemDto
                {
                    RecordId = r.RecordId,
                    AppointmentId = r.AppointmentId,
                    VisitAt = r.Appointment.AppointmentDate,

                    PatientId = r.Appointment.PatientId,
                    PatientName = r.Appointment.Patient.User.FullName ?? $"BN#{r.Appointment.PatientId}",
                    Gender = r.Appointment.Patient.User.Gender,
                    Dob = r.Appointment.Patient.User.Dob,
                    Phone = r.Appointment.Patient.User.Phone,

                    DiagnosisRaw = r.Diagnosis,

                    HasPrescription = presQ.Any(p => p.RecordId == r.RecordId),
                    LatestPrescriptionId = presQ
                        .Where(p => p.RecordId == r.RecordId)
                        .OrderByDescending(p => p.PrescriptionId)
                        .Select(p => (int?)p.PrescriptionId)
                        .FirstOrDefault()
                })
                .AsNoTracking()
                .ToListAsync(ct);

            return new PagedResult<RecordListItemDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total
            };
        }
    }
}
