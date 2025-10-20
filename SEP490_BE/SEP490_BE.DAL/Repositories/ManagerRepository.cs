using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories
{
    public class ManagerRepository : IManagerRepository
    {
        private readonly DiamondHealthContext _context;

        public ManagerRepository(DiamondHealthContext context)
        {
            _context = context;
        }

        public async Task<List<WorkScheduleDto>> GetWorkScheduleByDateAsync(DateOnly date)
        {
            var workSchedules = await _context.DoctorShifts
                .Where(ds => ds.EffectiveFrom <= date && (ds.EffectiveTo == null || ds.EffectiveTo >= date))
                .Include(ds => ds.Doctor)
                .ThenInclude(d => d.User)
                .Include(ds => ds.Doctor)
                .ThenInclude(d => d.Room)
                .Include(ds => ds.Shift)
                .Select(ds => new WorkScheduleDto
                {
                    DoctorShiftId = ds.DoctorShiftId,
                    DoctorId = ds.DoctorId,
                    DoctorName = ds.Doctor.User.FullName,
                    Specialty = ds.Doctor.Specialty,
                    RoomId = ds.Doctor.RoomId,
                    RoomName = ds.Doctor.Room.RoomName,
                    ShiftId = ds.ShiftId,
                    ShiftType = ds.Shift.ShiftType,
                    StartTime = ds.Shift.StartTime,
                    EndTime = ds.Shift.EndTime,
                    EffectiveFrom = ds.EffectiveFrom,
                    EffectiveTo = ds.EffectiveTo,
                    Status = ds.Status
                })
                .ToListAsync();

            return workSchedules;
        }

        public async Task<List<DailyWorkScheduleDto>> GetWorkScheduleByDateRangeAsync(DateOnly startDate, DateOnly endDate)
        {
            var workSchedules = await _context.DoctorShifts
                .Where(ds => ds.EffectiveFrom <= endDate && (ds.EffectiveTo == null || ds.EffectiveTo >= startDate))
                .Include(ds => ds.Doctor)
                .ThenInclude(d => d.User)
                .Include(ds => ds.Doctor)
                .ThenInclude(d => d.Room)
                .Include(ds => ds.Shift)
                .Select(ds => new WorkScheduleDto
                {
                    DoctorShiftId = ds.DoctorShiftId,
                    DoctorId = ds.DoctorId,
                    DoctorName = ds.Doctor.User.FullName,
                    Specialty = ds.Doctor.Specialty,
                    RoomId = ds.Doctor.RoomId,
                    RoomName = ds.Doctor.Room.RoomName,
                    ShiftId = ds.ShiftId,
                    ShiftType = ds.Shift.ShiftType,
                    StartTime = ds.Shift.StartTime,
                    EndTime = ds.Shift.EndTime,
                    EffectiveFrom = ds.EffectiveFrom,
                    EffectiveTo = ds.EffectiveTo,
                    Status = ds.Status
                })
                .ToListAsync();

            var dailySchedules = new List<DailyWorkScheduleDto>();
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                var daySchedules = workSchedules
                    .Where(ws => ws.EffectiveFrom <= currentDate && (ws.EffectiveTo == null || ws.EffectiveTo >= currentDate))
                    .ToList();

                dailySchedules.Add(new DailyWorkScheduleDto
                {
                    Date = currentDate,
                    Shifts = daySchedules
                });

                currentDate = currentDate.AddDays(1);
            }

            return dailySchedules;
        }
    }
}
