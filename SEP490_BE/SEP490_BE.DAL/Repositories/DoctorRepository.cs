using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.Repositories
{
    public class DoctorRepository: IDoctorRepository
    {
        private readonly DiamondHealthContext _context;

        public DoctorRepository(DiamondHealthContext context)
        {
            _context = context;
        }

        public async Task<List<Doctor>> GetAllDoctorsAsync()
        {
            return await _context.Doctors
                .Include(d => d.User)
                .ToListAsync();
        }
        public async Task<List<Doctor>> SearchDoctorsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return await GetAllDoctorsAsync();
            }

            keyword = keyword.Trim().ToLower();

            return await _context.Doctors
                .Include(d => d.User)
                .Where(d =>
                    d.User.FullName.ToLower().Contains(keyword) ||
                    d.Specialty.ToLower().Contains(keyword))
                .ToListAsync();
        }
        public async Task<List<DoctorActiveScheduleRangeDto>> GetDoctorActiveScheduleInRangeAsync(int doctorId, DateOnly startDate, DateOnly endDate)
        {
            var shifts = await _context.DoctorShifts
                .Include(ds => ds.Doctor).ThenInclude(d => d.User)
                .Include(ds => ds.Doctor).ThenInclude(d => d.Room)
                .Include(ds => ds.Shift)
                .Where(ds =>
                    ds.DoctorId == doctorId &&
                    ds.Status == "Active" &&
                    ds.EffectiveFrom <= endDate &&
                    (ds.EffectiveTo == null || ds.EffectiveTo >= startDate))
                .ToListAsync();

            var result = new List<DoctorActiveScheduleRangeDto>();

            foreach (var ds in shifts)
            {
                var from = ds.EffectiveFrom < startDate ? startDate : ds.EffectiveFrom;
                var to = ds.EffectiveTo == null || ds.EffectiveTo > endDate ? endDate : ds.EffectiveTo.Value;

                var current = from;
                while (current <= to)
                {
                    result.Add(new DoctorActiveScheduleRangeDto
                    {
                        DoctorId = ds.DoctorId,
                        DoctorName = ds.Doctor.User.FullName,
                        Specialty = ds.Doctor.Specialty,
                        RoomName = ds.Doctor.Room.RoomName,
                        Date = current,
                        ShiftType = ds.Shift.ShiftType,
                        StartTime = ds.Shift.StartTime,
                        EndTime = ds.Shift.EndTime,
                        Status = ds.Status
                    });
                    current = current.AddDays(1);
                }
            }

            return result.OrderBy(r => r.Date).ThenBy(r => r.StartTime).ToList();
        }
    }
}
