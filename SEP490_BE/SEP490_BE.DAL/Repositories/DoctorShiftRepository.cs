using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.Repositories
{
    public class DoctorShiftRepository : IDoctorShiftRepository
    {
        private readonly DiamondHealthContext _context;
        public DoctorShiftRepository(DiamondHealthContext context) => _context = context;

        public async Task<bool> IsShiftConflictAsync(
      int doctorId,
      int shiftId,
      DateOnly newFrom,
      DateOnly newTo)
        {
            return await _context.DoctorShifts.AnyAsync(ds =>
                ds.DoctorId == doctorId &&
                ds.ShiftId == shiftId &&
                ds.Status == "Active" &&
                // Kiểm tra giao nhau giữa hai khoảng [EffectiveFrom, EffectiveTo]
                ds.EffectiveFrom <= newTo && ds.EffectiveTo >= newFrom
            );
        }


        public async Task AddDoctorShiftAsync(DoctorShift entity)
        {
            _context.DoctorShifts.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<DoctorShift>> GetSchedulesAsync(DateOnly from, DateOnly to)
        {
            return await _context.DoctorShifts
                .Include(x => x.Shift)
                .Include(x => x.Doctor)
                .Where(x => x.EffectiveFrom >= from && x.EffectiveTo <= to)
                .ToListAsync();
        }
    }
}
