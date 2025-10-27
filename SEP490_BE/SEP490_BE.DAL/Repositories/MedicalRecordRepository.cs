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
                .Include(m => m.ObstetricRecord)
                .Include(m => m.PediatricRecord)
                .Include(m => m.Payments)
                .Include(m => m.Prescriptions)
                .Include(m => m.TestResults)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        // ✅ Lấy 1 MedicalRecord theo ID, kèm theo dữ liệu liên quan
        public async Task<MedicalRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.MedicalRecords
                .Include(m => m.Appointment)
                .Include(m => m.InternalMedRecord)
                .Include(m => m.ObstetricRecord)
                .Include(m => m.PediatricRecord)
                .Include(m => m.Payments)
                .Include(m => m.Prescriptions)
                .Include(m => m.TestResults)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.RecordId == id, cancellationToken);
        }
    }
}
