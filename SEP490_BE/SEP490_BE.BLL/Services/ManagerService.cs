using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.Repositories;

namespace SEP490_BE.BLL.Services
{
    public class ManagerService : IManagerService
    {
        private readonly IShiftRepository _shiftRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IDoctorShiftRepository _doctorShiftRepo;

        public ManagerService(
            IShiftRepository shiftRepo,
            IDoctorRepository doctorRepo,
            IDoctorShiftRepository doctorShiftRepo)
        {
            _shiftRepo = shiftRepo;
            _doctorRepo = doctorRepo;
            _doctorShiftRepo = doctorShiftRepo;
        }

        //  Danh sách ca làm việc
        public async Task<List<ShiftResponseDTO>> GetAllShiftsAsync()
        {
            var shifts = await _shiftRepo.GetAllShiftsAsync();
            return shifts.Select(s => new ShiftResponseDTO
            {
                ShiftID = s.ShiftId,
                ShiftType = s.ShiftType,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            }).ToList();
        }

        // Danh sách bác sĩ
        public async Task<List<DoctorDTO>> GetAllDoctorsAsync()
        {
            var doctors = await _doctorRepo.GetAllDoctorsAsync();
            return doctors.Select(d => new DoctorDTO
            {
                DoctorID = d.DoctorId,
                FullName = d.User.FullName,
                Specialty = d.Specialty,
                Email = d.User.Email
            }).ToList();
        }
        // Tìm bsi theo tên
        public async Task<List<DoctorDTO>> SearchDoctorsAsync(string keyword)
        {
            var doctors = await _doctorRepo.SearchDoctorsAsync(keyword);
            return doctors.Select(d => new DoctorDTO
            {
                DoctorID = d.DoctorId,
                FullName = d.User.FullName,
                Specialty = d.Specialty,
                Email = d.User.Email
            }).ToList();
        }

        // Kiểm tra trùng lịch bác sĩ
        public async Task<bool> CheckDoctorConflictAsync(int doctorId, int shiftId, DateOnly from, DateOnly to)
        {
            return await _doctorShiftRepo.IsShiftConflictAsync(doctorId, shiftId, from, to);
        }

        // Tạo lịch làm việc
        public async Task<int> CreateScheduleAsync(CreateScheduleRequestDTO dto)
        {
            int createdCount = 0;

            for (var date = dto.EffectiveFrom; date <= dto.EffectiveTo; date = date.AddDays(1))
            {
                foreach (var shift in dto.Shifts)
                {
                    foreach (var doctorId in shift.DoctorIds)
                    {
                        bool conflict = await _doctorShiftRepo.IsShiftConflictAsync(doctorId, shift.ShiftId, date, date);
                        if (!conflict)
                        {
                            var ds = new DoctorShift
                            {
                                DoctorId = doctorId,
                                ShiftId = shift.ShiftId,
                                EffectiveFrom = date,
                                EffectiveTo = date,
                                Status = "Active"
                            };
                            await _doctorShiftRepo.AddDoctorShiftAsync(ds);
                            createdCount++;
                        }
                    }
                }
            }
            return createdCount;
        }

        // Xem lịch làm việc đã tạo
        public async Task<List<DoctorShift>> GetSchedulesAsync(DateOnly from, DateOnly to)
        {
            return await _doctorShiftRepo.GetSchedulesAsync(from, to);
        }
    }
}
