using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.Repositories
{
    public class MedicalRecordRepository : IMedicalRecordRepository
    {
        private readonly DiamondHealthContext _context;

        public MedicalRecordRepository(DiamondHealthContext context)
        {
            _context = context;
        }

        // ✅ Lấy tất cả MedicalRecord, bao gồm các bảng liên quan
        public async Task<List<MedicalRecord>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.MedicalRecords
                .Include(m => m.Appointment)
                .Include(m => m.InternalMedRecord)
                .Include(m => m.DermatologyRecords)
                .Include(m => m.PediatricRecord)
                .Include(m => m.Payments)
                .Include(m => m.Prescriptions)
                .Include(m => m.TestResults)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<List<MedicalRecord>> GetAllByDoctorAsync(int id, CancellationToken cancellationToken = default)
        {
            var doctorId = await GetDoctorIdByUserIdAsync(id, cancellationToken);
            return await _context.MedicalRecords
                .Include(m => m.Appointment)
                    .ThenInclude(a => a.Doctor)
                .Include(m => m.InternalMedRecord)
                .Include(m => m.DermatologyRecords)
                .Include(m => m.PediatricRecord)
                .Include(m => m.Payments)
                .Include(m => m.Prescriptions)
                .Include(m => m.TestResults)
                .Where(m => m.Appointment.DoctorId == doctorId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        // ✅ Lấy 1 MedicalRecord theo ID, kèm theo dữ liệu liên quan
        public async Task<MedicalRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.MedicalRecords
                .Include(m => m.Appointment)
                .Include(m => m.InternalMedRecord)
                .Include(m => m.DermatologyRecords)
                .Include(m => m.PediatricRecord)
                .Include(m => m.Payments)
                .Include(m => m.Prescriptions)
                .Include(m => m.TestResults)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.RecordId == id, cancellationToken);
        }

        public async Task<MedicalRecord> CreateAsync(MedicalRecord record, CancellationToken cancellationToken = default)
        {
            await _context.MedicalRecords.AddAsync(record, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return record;
        }

        public async Task<MedicalRecord?> UpdateAsync(int id, string? doctorNotes, string? diagnosis, CancellationToken cancellationToken = default)
        {
            var entity = await _context.MedicalRecords.FirstOrDefaultAsync(x => x.RecordId == id, cancellationToken);
            if (entity == null) return null;

            entity.DoctorNotes = doctorNotes;
            entity.Diagnosis = diagnosis;
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default)
        {
            return await _context.MedicalRecords
                .Include(m => m.Appointment)
                .Include(m => m.InternalMedRecord)
                .Include(m => m.DermatologyRecords)
                .Include(m => m.PediatricRecord)
                .Include(m => m.Payments)
                .Include(m => m.Prescriptions)
                .Include(m => m.TestResults)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.AppointmentId == appointmentId, cancellationToken);
        }

        public async Task<int?> GetDoctorIdByUserIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Doctors
                .AsNoTracking()
                .Where(d => d.UserId == id)
                .Select(d => (int?)d.DoctorId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> IsRecordOwnedByDoctorAsync(int recordId, int userId, CancellationToken cancellationToken = default)
        {
            var doctorId = await GetDoctorIdByUserIdAsync(userId, cancellationToken);
            if (doctorId == null) return false;

            return await _context.MedicalRecords
                .AsNoTracking()
                .Include(m => m.Appointment)
                .AnyAsync(m => m.RecordId == recordId && m.Appointment.DoctorId == doctorId, cancellationToken);
        }

        public async Task<bool> IsAppointmentOwnedByDoctorAsync(int appointmentId, int userId, CancellationToken cancellationToken = default)
        {
            var doctorId = await GetDoctorIdByUserIdAsync(userId, cancellationToken);
            if (doctorId == null) return false;

            return await _context.Appointments
                .AsNoTracking()
                .AnyAsync(a => a.AppointmentId == appointmentId && a.DoctorId == doctorId, cancellationToken);
        }
    }
}
