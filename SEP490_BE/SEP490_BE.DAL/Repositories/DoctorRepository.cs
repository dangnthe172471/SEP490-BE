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
        public async Task<List<DoctorActiveScheduleRangeDto>> GetDoctorActiveScheduleInRangeAsync(
       int doctorId, DateOnly startDate, DateOnly endDate)
        {
            // Ca làm việc dang hoat dong
            var baseShifts = await _context.DoctorShifts
                .Where(ds =>
                    ds.Status == "Active" &&
                    ds.EffectiveFrom <= endDate &&
                    (ds.EffectiveTo == null || ds.EffectiveTo >= startDate))
                .Select(ds => new
                {
                    ds.DoctorShiftId,
                    ds.DoctorId,
                    DoctorName = ds.Doctor.User.FullName,
                    ds.Doctor.Specialty,
                    RoomName = ds.Doctor.Room.RoomName,
                    ds.Shift.ShiftType,
                    ds.Shift.StartTime,
                    ds.Shift.EndTime,
                    ds.EffectiveFrom,
                    ds.EffectiveTo
                })
                .ToListAsync();

           
            var approvedExchanges = await _context.DoctorShiftExchanges
                .Where(e => e.Status == "Approved" &&
                            e.ExchangeDate >= startDate &&
                            e.ExchangeDate <= endDate)
                .Select(e => new
                {
                    e.Doctor1ShiftRefId,
                    e.Doctor2ShiftRefId,
                    e.ExchangeDate
                })
                .ToListAsync();

            // Override lịch theo ngày va giờ bắt đầu
            var schedule = new Dictionary<(DateOnly Date, TimeOnly StartTime), DoctorActiveScheduleRangeDto>();

            //Lịch gốc
            foreach (var s in baseShifts.Where(x => x.DoctorId == doctorId))
            {
                var from = s.EffectiveFrom < startDate ? startDate : s.EffectiveFrom;
                var to = s.EffectiveTo == null || s.EffectiveTo > endDate ? endDate : s.EffectiveTo.Value;

                for (var d = from; d <= to; d = d.AddDays(1))
                {
                    var key = (d, s.StartTime);
                    schedule[key] = new DoctorActiveScheduleRangeDto
                    {
                        DoctorId = s.DoctorId,
                        DoctorName = s.DoctorName,
                        Specialty = s.Specialty,
                        RoomName = s.RoomName,
                        Date = d,
                        ShiftType = s.ShiftType,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        Status = "Active"
                    };
                }
            }

            // Áp các Exchange 
            foreach (var ex in approvedExchanges)
            {
                var exDate = ex.ExchangeDate!.Value;

                // 2 ca được đổi
                var s1 = baseShifts.FirstOrDefault(s => s.DoctorShiftId == ex.Doctor1ShiftRefId);
                var s2 = baseShifts.FirstOrDefault(s => s.DoctorShiftId == ex.Doctor2ShiftRefId);

                if (s1 == null || s2 == null) continue; 

          
                if (s1.DoctorId == doctorId)
                {
                    // xoa ca cu 
                    schedule.Remove((exDate, s1.StartTime));

                    // Thêm ca cua nguoi doi 2 vao lich 1
                    schedule[(exDate, s2.StartTime)] = new DoctorActiveScheduleRangeDto
                    {
                        DoctorId = doctorId,
                        DoctorName = s1.DoctorName,
                        Specialty = s1.Specialty,
                        RoomName = s2.RoomName,
                        Date = exDate,
                        ShiftType = s2.ShiftType,
                        StartTime = s2.StartTime,
                        EndTime = s2.EndTime,
                        Status = "Exchange"
                    };
                }
                else if (s2.DoctorId == doctorId)
                {
                    schedule.Remove((exDate, s2.StartTime));

                    schedule[(exDate, s1.StartTime)] = new DoctorActiveScheduleRangeDto
                    {
                        DoctorId = doctorId,
                        DoctorName = s2.DoctorName,
                        Specialty = s2.Specialty,
                        RoomName = s1.RoomName,
                        Date = exDate,
                        ShiftType = s1.ShiftType,
                        StartTime = s1.StartTime,
                        EndTime = s1.EndTime,
                        Status = "Exchange"
                    };
                }
            }
            return schedule.Values
                .OrderBy(x => x.Date)
                .ThenBy(x => x.StartTime)
                .ToList();
        }

        public async Task<List<DoctorActiveScheduleRangeDto>> GetAllDoctorSchedulesInRangeAsync(
         DateOnly startDate, DateOnly endDate)
        {
            var doctorIds = await _context.Doctors
                .Include(d => d.User)
                .Where(d => d.User.IsActive == true)
                .Select(d => d.DoctorId)
                .ToListAsync();

            var all = new List<DoctorActiveScheduleRangeDto>();

            foreach (var id in doctorIds)
            {
                var each = await GetDoctorActiveScheduleInRangeAsync(id, startDate, endDate);
                all.AddRange(each);
            }

            return all
                .OrderBy(x => x.DoctorName)
                .ThenBy(x => x.Date)
                .ThenBy(x => x.StartTime)
                .ToList();
        }
        public async Task<List<int>> GetUserIdsByDoctorIdsAsync(List<int> doctorIds)
        {
            return await _context.Doctors
                .Where(d => doctorIds.Contains(d.DoctorId))
                .Select(d => d.UserId)
                .Distinct()
                .ToListAsync();
        }

    }
}
