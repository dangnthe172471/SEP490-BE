using SEP490_BE.BLL.IServices.IManagerServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.DTOs.ManagerDTO.ManagerSchedule;
using SEP490_BE.DAL.DTOs.MedicineDTO;
using SEP490_BE.DAL.Helpers;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.IRepositories.IManagerRepositories;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Services.ManagerServices
{
    public class ScheduleService : IScheduleService
    {
        private readonly IShiftRepository _shiftRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IDoctorShiftRepository _doctorShiftRepo;
        private readonly IScheduleRepository _managerRepo;

        public ScheduleService(
            IShiftRepository shiftRepo,
            IDoctorRepository doctorRepo,
            IDoctorShiftRepository doctorShiftRepo,
            IScheduleRepository managerRepo)
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

        public async Task<PaginationHelper.PagedResult<WorkScheduleDto>> GetAllSchedulesAsync(int pageNumber, int pageSize)
        {
            return await _managerRepo.GetAllSchedulesAsync(pageNumber, pageSize);
        }
        public async Task<List<DailyWorkScheduleViewDto>> GetWorkScheduleByDateRangeAsync(DateOnly startDate, DateOnly endDate)
        {
            var schedules = await _managerRepo.GetWorkScheduleByDateRangeAsync(startDate, endDate);
            return schedules.OrderBy(s => s.Date).ToList();
        }
        public async Task<PaginationHelper.PagedResult<DailyWorkScheduleDto>> GetWorkSchedulesByDateAsync(DateOnly? date, int pageNumber, int pageSize)
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
        public async Task<List<DailySummaryDto>> GetMonthlyWorkSummaryAsync(int year, int month)
        {
            if (year <= 0 || month <= 0 || month > 12)
                throw new ArgumentException("Tháng hoặc năm không hợp lệ");

            return await _managerRepo.GetMonthlyWorkSummaryAsync(year, month);
        }
        public async Task<PaginationHelper.PagedResult<WorkScheduleGroupDto>> GetGroupedWorkScheduleListAsync(
      int pageNumber, int pageSize)
        {
            // Lấy toàn bộ lịch (DoctorShift)
            var list = await _managerRepo.GetAllWorkSchedulesAsync(null, null);

            //Nếu EffectiveTo = null → tự gán = EffectiveFrom + 1 tháng
            var adjustedList = list.Select(x => new
            {
             x.DoctorShiftId,
                x.DoctorId,
                x.DoctorName,
                x.Specialty,
                x.ShiftId,
                x.ShiftType,
                x.StartTime,
                x.EndTime,
                x.EffectiveFrom,
                EffectiveTo = x.EffectiveTo ?? x.EffectiveFrom.AddMonths(1)
            }).ToList();

            //  Gom nhóm theo khoảng thời gian (không group theo Shift)
            var grouped = adjustedList
                .GroupBy(x => new { x.EffectiveFrom, x.EffectiveTo })
                .Select(g => new WorkScheduleGroupDto
                {
                    EffectiveFrom = g.Key.EffectiveFrom,
                    EffectiveTo = g.Key.EffectiveTo,

                    //  Gộp nhiều ca trong cùng khoảng thời gian
                    Shifts = g.GroupBy(s => s.ShiftId)
                    .OrderByDescending(sg => sg.Max(x => x.DoctorShiftId))
                        .Select(sg => new ShiftResponseDto
                        {
                            ShiftID = sg.Key,
                            ShiftType = sg.First().ShiftType,
                            StartTime = sg.First().StartTime.ToString("HH:mm:ss"),
                            EndTime = sg.First().EndTime.ToString("HH:mm:ss"),
                            Doctors = sg
                            .OrderByDescending(d => d.DoctorShiftId)
                            .Select(d => new DoctorDTO
                            {
                                DoctorID = d.DoctorId,
                                FullName = d.DoctorName,
                                Specialty = d.Specialty,
                                Email = $"{d.DoctorName}@example.com"
                            }).ToList()
                        }).ToList()
                })
                .OrderByDescending(g => g.EffectiveFrom)
                .ToList();

          
            var totalCount = grouped.Count;
            var items = grouped.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            
            return new PaginationHelper.PagedResult<WorkScheduleGroupDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        public async Task UpdateDoctorShiftsInRangeAsync(UpdateDoctorShiftRangeRequest request)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var newToDate = request.NewToDate ?? request.ToDate;

            // Lấy nhóm hiện tại (chính xác theo from - to)
            var existing = await _managerRepo.GetExactRangeAsync(request.ShiftId, request.FromDate, request.ToDate);

            if (!existing.Any())
                throw new Exception("Không tìm thấy nhóm lịch tương ứng để cập nhật.");

            //  ĐỔI ToDate (tạo nhóm mới) 
            if (newToDate != request.ToDate)
            {
                // 1. Inactivate nhóm cũ
                foreach (var e in existing)
                {
                    e.Status = "Inactive";
                    await _managerRepo.UpdateAsync(e);
                }

                // 2. Tạo nhóm mới với ToDate mới
                var allDoctorIds = existing.Select(e => e.DoctorId)
                    .Union(request.AddDoctorIds)
                    .Except(request.RemoveDoctorIds)
                    .Distinct()
                    .ToList();

                foreach (var doctorId in allDoctorIds)
                {
                    var newShift = new DoctorShift
                    {
                        DoctorId = doctorId,
                        ShiftId = request.ShiftId,
                        EffectiveFrom = request.FromDate < today ? today : request.FromDate,
                        EffectiveTo = newToDate,
                        Status = "Active"
                    };
                    await _managerRepo.AddAsync(newShift);
                }
            }
            else
            {
                //  KHÔNG ĐỔI ToDate, chỉ thêm/xóa bác sĩ 
                foreach (var id in request.RemoveDoctorIds)
                {
                    var target = existing.FirstOrDefault(x => x.DoctorId == id);
                    if (target != null)
                    {
                        target.EffectiveTo = today.AddDays(-1);
                        target.Status = "Inactive";
                        await _managerRepo.UpdateAsync(target);
                    }
                }

                foreach (var id in request.AddDoctorIds)
                {
                    if (!existing.Any(x => x.DoctorId == id))
                    {
                        var newShift = new DoctorShift
                        {
                            DoctorId = id,
                            ShiftId = request.ShiftId,
                            EffectiveFrom = request.FromDate < today ? today : request.FromDate,
                            EffectiveTo = request.ToDate,
                            Status = "Active"
                        };
                        await _managerRepo.AddAsync(newShift);
                    }
                }
            }

            await _managerRepo.SaveChangesAsync();
        }


        public async Task<bool> CheckDoctorShiftLimitAsync(int doctorId, DateOnly date)
        {
            
            return await _managerRepo.CheckDoctorShiftLimitAsync(doctorId, date);
        }

        public async Task<bool> CheckDoctorShiftLimitRangeAsync(int doctorId, DateOnly from, DateOnly to)
        {
            return await _managerRepo.CheckDoctorShiftLimitRangeAsync(doctorId, from, to);
        }
        //       public async Task RefreshShiftStatusAsync()
        //       {
        //           var today = DateTime.Today;


        //           var expiredShifts = await _managerRepo.GetAllAsync(
        //    ds => ds.Status == "Active" &&
        //          ds.EffectiveTo != null &&
        //          ds.EffectiveTo.Value.ToDateTime(TimeOnly.MinValue) < today
        //);

        //           if (!expiredShifts.Any()) return;

        //           foreach (var shift in expiredShifts)
        //           {
        //               shift.Status = "Inactive";
        //               await _managerRepo.UpdateAsync(shift);
        //           }

        //           await _managerRepo.SaveChangesAsync();
        //       }
    }
}
