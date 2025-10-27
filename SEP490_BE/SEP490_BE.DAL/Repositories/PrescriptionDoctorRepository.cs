using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories
{
    public class PrescriptionDoctorRepository : IPrescriptionDoctorRepository
    {
        private readonly DiamondHealthContext _db;
        public PrescriptionDoctorRepository(DiamondHealthContext db) => _db = db;

        public Task<Doctor?> GetDoctorByUserIdAsync(int userId, CancellationToken ct)
            => _db.Doctors.Include(d => d.User)
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

        public async Task<Dictionary<int, Medicine>> GetMedicinesByIdsAsync(IEnumerable<int> ids, CancellationToken ct)
        {
            var arr = ids.Distinct().ToArray();
            var meds = await _db.Medicines
                .Include(m => m.Provider) // 🔹 lấy luôn nhà cung cấp
                .Where(m => arr.Contains(m.MedicineId))
                .ToListAsync(ct);

            return meds.ToDictionary(m => m.MedicineId, m => m);
        }

        public async Task<Prescription> CreatePrescriptionAsync(
            Prescription header, IEnumerable<PrescriptionDetail> details, CancellationToken ct)
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
                      .ThenInclude(d => d.Medicine)
                          .ThenInclude(m => m.Provider) // 🔹 lấy luôn Provider
                  .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId, ct);
    }
}
