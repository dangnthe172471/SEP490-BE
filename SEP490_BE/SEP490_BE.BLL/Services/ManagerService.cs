using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.DTOs.MedicineDTO;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Services
{
    public class ManagerService : IManagerService
    {
        private readonly IShiftRepository _shiftRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IDoctorShiftRepository _doctorShiftRepo;
        private readonly IManagerRepository _managerRepo;

        public ManagerService(
            IShiftRepository shiftRepo,
            IDoctorRepository doctorRepo,
            IDoctorShiftRepository doctorShiftRepo,
            IManagerRepository managerRepo)
        {
            _shiftRepo = shiftRepo;
            _doctorRepo = doctorRepo;
            _doctorShiftRepo = doctorShiftRepo;
            _managerRepo = managerRepo;
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

            foreach (var shift in dto.Shifts)
            {
                foreach (var doctorId in shift.DoctorIds)
                {
                    bool conflict = await _doctorShiftRepo.IsShiftConflictAsync(
                        doctorId,
                        shift.ShiftId,
                        dto.EffectiveFrom,
                        dto.EffectiveTo
                    );

                    if (!conflict)
                    {
                        var ds = new DoctorShift
                        {
                            DoctorId = doctorId,
                            ShiftId = shift.ShiftId,
                            EffectiveFrom = dto.EffectiveFrom,
                            EffectiveTo = dto.EffectiveTo,
                            Status = "Active"
                        };

                        await _doctorShiftRepo.AddDoctorShiftAsync(ds);
                        createdCount++;
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

        public async Task<PagedResult<WorkScheduleDto>> GetAllSchedulesAsync(int pageNumber, int pageSize)
        {
            return await _managerRepo.GetAllSchedulesAsync(pageNumber, pageSize);
        }
        public async Task<List<DailyWorkScheduleViewDto>> GetWorkScheduleByDateRangeAsync(DateOnly startDate, DateOnly endDate)
        {
            var schedules = await _managerRepo.GetWorkScheduleByDateRangeAsync(startDate, endDate);
            return schedules.OrderBy(s => s.Date).ToList();
        }
        public async Task<PagedResult<DailyWorkScheduleDto>> GetWorkSchedulesByDateAsync(DateOnly? date, int pageNumber, int pageSize)
        {
            return await _managerRepo.GetWorkSchedulesByDateAsync(date, pageNumber, pageSize);
        }

        public async Task UpdateWorkScheduleByDateAsync(UpdateWorkScheduleByDateRequest request)
        {
            await _managerRepo.UpdateWorkScheduleByDateAsync(request);
        }

        public async Task UpdateWorkScheduleByIdAsync(UpdateWorkScheduleByIdRequest request)
        {
            await _managerRepo.UpdateWorkScheduleByIdAsync(request);
        }
    }
}
