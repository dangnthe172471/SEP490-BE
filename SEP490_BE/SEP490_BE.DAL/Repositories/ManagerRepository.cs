using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.DTOs.ManagerDTO.ManagerSchedule;
using SEP490_BE.DAL.DTOs.MedicineDTO;
using SEP490_BE.DAL.Helpers;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SEP490_BE.DAL.Repositories
{
    public class ManagerRepository : IManagerRepository
    {
        private readonly DiamondHealthContext _context;

        public ManagerRepository(DiamondHealthContext context)
        {
            _context = context;
        }

        public async Task<PaginationHelper.PagedResult<DailyWorkScheduleDto>> GetWorkSchedulesByDateAsync(DateOnly? date, int pageNumber, int pageSize)
        {
            var query = _context.DoctorShifts
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
                });

            //  Nếu có truyền date, lọc theo ngày
            if (date.HasValue)
            {
                query = query.Where(ds =>
                    ds.EffectiveFrom == date.Value &&
                    (ds.EffectiveTo == null || ds.EffectiveTo >= date.Value));
            }

            //  Group theo ngày
            var groupedQuery = query
                .GroupBy(x => x.EffectiveFrom)
                .Select(g => new DailyWorkScheduleDto
                {
                    Date = g.Key,
                    Shifts = g.ToList()
                })
                .OrderBy(x => x.Date);

            //  Phân trang
            return await groupedQuery.ToPagedResultAsync(pageNumber, pageSize);
        }

        #region Lấy lịch theo range nếu effectTo = null là vĩnh viễn 
        //public async Task<List<DailyWorkScheduleViewDto>> GetWorkScheduleByDateRangeAsync(DateOnly startDate, DateOnly endDate)
        //{
        //    if (startDate == default || endDate == default)
        //    {
        //        var today = DateOnly.FromDateTime(DateTime.Now);
        //        startDate = new DateOnly(today.Year, today.Month, 1);
        //        endDate = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
        //    }


        //    var dailySchedules = new List<DailyWorkScheduleViewDto>();

        //    var shifts = await _context.Shifts.ToListAsync(); // cache shift definitions
        //    var currentDate = startDate;

        //    while (currentDate <= endDate)
        //    {
        //        // chỉ lấy DoctorShift hiệu lực đúng ngày này
        //        var dayShifts = await _context.DoctorShifts
        //            .Where(ds => ds.EffectiveFrom <= currentDate &&
        //                        (ds.EffectiveTo == null || ds.EffectiveTo >= currentDate))
        //            .Include(ds => ds.Doctor).ThenInclude(d => d.User)
        //            .Include(ds => ds.Doctor).ThenInclude(d => d.Room)
        //            .Include(ds => ds.Shift)
        //            .ToListAsync();

        //        var groupedByShift = dayShifts
        //            .GroupBy(ws => ws.ShiftId)
        //            .Select(g =>
        //            {
        //                var first = g.First();
        //                return new ShiftResponseDto
        //                {
        //                    ShiftID = first.ShiftId,
        //                    ShiftType = first.Shift.ShiftType,
        //                    StartTime = first.Shift.StartTime.ToString(@"HH\:mm"),
        //                    EndTime = first.Shift.EndTime.ToString(@"HH\:mm"),
        //                    Doctors = g.Select(x => new DoctorDTO
        //                    {
        //                        DoctorID = x.DoctorId,
        //                        FullName = x.Doctor.User.FullName,
        //                        Specialty = x.Doctor.Specialty ?? "",
        //                        Email = x.Doctor.User.Email
        //                    }).DistinctBy(d => d.DoctorID).ToList()
        //                };
        //            })
        //            .OrderBy(s => s.StartTime)
        //            .ToList();

        //        dailySchedules.Add(new DailyWorkScheduleViewDto
        //        {
        //            Date = currentDate,
        //            Shifts = groupedByShift
        //        });

        //        currentDate = currentDate.AddDays(1);
        //    }

        //    return dailySchedules;
        //}
        #endregion

        // Lấy lịch theo range nếu effectTo = null là 30 ngày kể từ effectiveFrom
        public async Task<List<DailyWorkScheduleViewDto>> GetWorkScheduleByDateRangeAsync(DateOnly startDate, DateOnly endDate)
        {

            if (startDate == default || endDate == default)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                startDate = new DateOnly(today.Year, today.Month, 1);
                endDate = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            }

            var dailySchedules = new List<DailyWorkScheduleViewDto>();


            var shifts = await _context.Shifts.ToListAsync();
            var workSchedules = await _context.DoctorShifts
                .Include(ds => ds.Doctor).ThenInclude(d => d.User)
                .Include(ds => ds.Doctor).ThenInclude(d => d.Room)
                .Include(ds => ds.Shift)
                .Where(ds =>
                //ds.Status == "Active" &&
                    ds.EffectiveFrom <= endDate &&
                    (ds.EffectiveTo == null || ds.EffectiveTo >= startDate))
                .ToListAsync();


            foreach (var ws in workSchedules)
            {
                if (ws.EffectiveTo == null)
                {
                    ws.EffectiveTo = ws.EffectiveFrom.AddMonths(1);
                }
            }

            var currentDate = startDate;

            while (currentDate <= endDate)
            {


                var dayShifts = workSchedules
                    .Where(ds => ds.EffectiveFrom <= currentDate &&
                                 ds.EffectiveTo >= currentDate)
                    .ToList();

                var groupedByShift = dayShifts
                    .GroupBy(ws => ws.ShiftId)
                    .Select(g =>
                    {
                        var first = g.First();
                        return new ShiftResponseDto
                        {
                            ShiftID = first.ShiftId,
                            ShiftType = first.Shift.ShiftType,
                            StartTime = first.Shift.StartTime.ToString(@"HH\:mm"),
                            EndTime = first.Shift.EndTime.ToString(@"HH\:mm"),
                            Doctors = g.Select(x => new DoctorDTO
                            {
                                DoctorID = x.DoctorId,
                                FullName = x.Doctor.User.FullName,
                                Specialty = x.Doctor.Specialty ?? "",
                                Email = x.Doctor.User.Email
                            }).DistinctBy(d => d.DoctorID).ToList()
                        };
                    })
                    .OrderBy(s => s.StartTime)
                    .ToList();

                dailySchedules.Add(new DailyWorkScheduleViewDto
                {
                    Date = currentDate,
                    Shifts = groupedByShift
                });

                currentDate = currentDate.AddDays(1);
            }

            return dailySchedules;
        }


        public async Task<PaginationHelper.PagedResult<WorkScheduleDto>> GetAllSchedulesAsync(int pageNumber, int pageSize)
        {
            var query = _context.DoctorShifts
                .Include(ds => ds.Doctor)
                    .ThenInclude(ds => ds.Room)
                .Include(ds => ds.Doctor)
                    .ThenInclude(d => d.User)
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
                }).OrderBy(ds => ds.EffectiveFrom);
            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }
        public async Task UpdateWorkScheduleByDateAsync(UpdateWorkScheduleByDateRequest request)
        {
            try
            {
                // Lấy tất cả ca làm việc hiệu lực trong ngày được chọn (và còn Active)
                var existingShifts = await _context.DoctorShifts
                    .Where(ds => ds.ShiftId == request.ShiftId &&
                                 ds.Status == "Active" &&
                                 ds.EffectiveFrom <= request.Date &&
                                 (ds.EffectiveTo == null || ds.EffectiveTo >= request.Date))
                    .ToListAsync();

                // Cập nhật trạng thái Inactive cho các bác sĩ bị xóa khỏi ca
                foreach (var doctorId in request.RemoveDoctorIds)
                {
                    var toUpdate = existingShifts.FirstOrDefault(s => s.DoctorId == doctorId);
                    if (toUpdate != null)
                    {
                        // Kiểm tra xem lịch này có đang được tham chiếu trong bảng hoán đổi không
                        bool hasExchange = await _context.DoctorShiftExchanges
                            .AnyAsync(e =>
                                e.Doctor1ShiftRefId == toUpdate.DoctorShiftId ||
                                e.Doctor2ShiftRefId == toUpdate.DoctorShiftId);

                        if (hasExchange)
                            throw new Exception($"Không thể chỉnh lịch bác sĩ ID={doctorId} vì đang có lịch hoán đổi ca!");

                        // Nếu ca kéo dài nhiều ngày và ngày cần chỉnh nằm giữa → chia ra 2 đoạn Active, đoạn giữa Inactive
                        if (toUpdate.EffectiveFrom < request.Date && toUpdate.EffectiveTo > request.Date)
                        {
                            var before = new DoctorShift
                            {
                                DoctorId = toUpdate.DoctorId,
                                ShiftId = toUpdate.ShiftId,
                                EffectiveFrom = toUpdate.EffectiveFrom,
                                EffectiveTo = request.Date.AddDays(-1),
                                Status = "Active"
                            };

                            var after = new DoctorShift
                            {
                                DoctorId = toUpdate.DoctorId,
                                ShiftId = toUpdate.ShiftId,
                                EffectiveFrom = request.Date.AddDays(1),
                                EffectiveTo = toUpdate.EffectiveTo,
                                Status = "Active"
                            };

                            // Đánh dấu bản gốc là Inactive
                            toUpdate.Status = "Inactive";
                            _context.DoctorShifts.Update(toUpdate);
                            await _context.DoctorShifts.AddRangeAsync(before, after);
                        }
                        else if (toUpdate.EffectiveFrom == request.Date && toUpdate.EffectiveTo > request.Date)
                        {
                            // Cắt phần đầu range
                            toUpdate.Status = "Inactive";

                            var after = new DoctorShift
                            {
                                DoctorId = toUpdate.DoctorId,
                                ShiftId = toUpdate.ShiftId,
                                EffectiveFrom = request.Date.AddDays(1),
                                EffectiveTo = toUpdate.EffectiveTo,
                                Status = "Active"
                            };

                            _context.DoctorShifts.Update(toUpdate);
                            await _context.DoctorShifts.AddAsync(after);
                        }
                        else if (toUpdate.EffectiveFrom < request.Date && toUpdate.EffectiveTo == request.Date)
                        {
                            // Cắt phần cuối range
                            toUpdate.Status = "Inactive";

                            var before = new DoctorShift
                            {
                                DoctorId = toUpdate.DoctorId,
                                ShiftId = toUpdate.ShiftId,
                                EffectiveFrom = toUpdate.EffectiveFrom,
                                EffectiveTo = request.Date.AddDays(-1),
                                Status = "Active"
                            };

                            _context.DoctorShifts.Update(toUpdate);
                            await _context.DoctorShifts.AddAsync(before);
                        }
                        else
                        {
                            // Trường hợp chỉ 1 ngày hoặc trùng chính xác → chỉ cần đổi trạng thái
                            toUpdate.Status = "Inactive";
                            _context.DoctorShifts.Update(toUpdate);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                //  Thêm bác sĩ mới vào ca trong ngày được chọn
                foreach (var doctorId in request.AddDoctorIds)
                {
                    bool doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorId == doctorId);
                    bool shiftExists = await _context.Shifts.AnyAsync(s => s.ShiftId == request.ShiftId);
                    if (!doctorExists || !shiftExists)
                        throw new Exception($"DoctorId hoặc ShiftId không hợp lệ (DoctorId={doctorId}, ShiftId={request.ShiftId})");

                    // Kiểm tra xem bác sĩ đó đã có ca Active trong khoảng bao gồm ngày đó chưa
                    bool duplicate = await _context.DoctorShifts.AnyAsync(x =>
                        x.DoctorId == doctorId &&
                        x.ShiftId == request.ShiftId &&
                        x.Status == "Active" &&
                        x.EffectiveFrom <= request.Date &&
                        (x.EffectiveTo == null || x.EffectiveTo >= request.Date));

                    if (!duplicate)
                    {
                        var newShift = new DoctorShift
                        {
                            DoctorId = doctorId,
                            ShiftId = request.ShiftId,
                            EffectiveFrom = request.Date,
                            EffectiveTo = request.Date,
                            Status = "Active"
                        };

                        await _context.DoctorShifts.AddAsync(newShift);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateWorkScheduleByDateAsync error: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("Inner: " + ex.InnerException.Message);

                throw;
            }
        }



        // Cập nhật lịch theo ID (DoctorShiftId)
        public async Task UpdateWorkScheduleByIdAsync(UpdateWorkScheduleByIdRequest request)
        {
            try
            {
                var entity = await _context.DoctorShifts
                    .FirstOrDefaultAsync(ds => ds.DoctorShiftId == request.DoctorShiftId);

                if (entity == null)
                    throw new Exception($"Không tìm thấy lịch có ID = {request.DoctorShiftId}");

                if (request.NewDoctorId.HasValue)
                {
                    bool doctorExists = await _context.Doctors
                        .AnyAsync(d => d.DoctorId == request.NewDoctorId.Value);

                    if (!doctorExists)
                        throw new Exception($"DoctorId {request.NewDoctorId.Value} không tồn tại!");

                    entity.DoctorId = request.NewDoctorId.Value;
                }

                if (request.NewShiftId.HasValue)
                {
                    bool shiftExists = await _context.Shifts
                        .AnyAsync(s => s.ShiftId == request.NewShiftId.Value);

                    if (!shiftExists)
                        throw new Exception($"ShiftId {request.NewShiftId.Value} không tồn tại!");

                    entity.ShiftId = request.NewShiftId.Value;
                }

                if (request.NewEffectiveFrom.HasValue)
                    entity.EffectiveFrom = request.NewEffectiveFrom.Value;

                if (request.NewEffectiveTo.HasValue)
                    entity.EffectiveTo = request.NewEffectiveTo.Value;

                if (!string.IsNullOrEmpty(request.Status))
                    entity.Status = request.Status;

                _context.DoctorShifts.Update(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật lịch ID={request.DoctorShiftId}: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");

                throw new Exception($"Cập nhật lịch thất bại: {ex.Message}");
            }
        }
        #region Tổng lịch nếu effectTo = null là vĩnh viễn 

        //public async Task<List<DailySummaryDto>> GetMonthlyWorkSummaryAsync(int year, int month)
        //{
        //    var startDate = new DateOnly(year, month, 1);
        //    var endDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

        //    var query = _context.DoctorShifts
        //        .Include(ds => ds.Doctor)
        //        .Where(ds => ds.EffectiveFrom <= endDate &&
        //                     (ds.EffectiveTo == null || ds.EffectiveTo >= startDate));

        //    var result = await query
        //        .GroupBy(ds => ds.EffectiveFrom)
        //        .Select(g => new DailySummaryDto
        //        {
        //            Date = g.Key,
        //            ShiftCount = g.Select(x => x.ShiftId).Distinct().Count(),
        //            DoctorCount = g.Select(x => x.DoctorId).Distinct().Count()
        //        })
        //        .OrderBy(x => x.Date)
        //        .ToListAsync();

        //    return result;
        //}
        #endregion
        public async Task<List<DailySummaryDto>> GetMonthlyWorkSummaryAsync(int year, int month)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

            // Lấy toàn bộ Active
            var shifts = await _context.DoctorShifts
                .Include(ds => ds.Doctor)
                .Where(ds => ds.Status == "Active")
                .ToListAsync();

            //Lọc
            var filtered = shifts.Where(ds =>
                ds.EffectiveFrom <= endDate &&
                (
                    (ds.EffectiveTo != null && ds.EffectiveTo >= startDate)
                    || (ds.EffectiveTo == null && ds.EffectiveFrom.AddMonths(1) >= startDate)
                )
            ).ToList();

            //Tạo danh sách lịch theo từng ngày trong tháng
            var expanded = new List<(DateOnly Date, int ShiftId, int DoctorId)>();
            foreach (var ds in filtered)
            {
                var from = ds.EffectiveFrom < startDate ? startDate : ds.EffectiveFrom;
                var to = ds.EffectiveTo != null
                    ? (ds.EffectiveTo > endDate ? endDate : ds.EffectiveTo.Value)
                    : (ds.EffectiveFrom.AddMonths(1) > endDate ? endDate : ds.EffectiveFrom.AddMonths(1));

                for (var d = from; d <= to; d = d.AddDays(1))
                {
                    expanded.Add((d, ds.ShiftId, ds.DoctorId));
                }
            }

            //Gom nhóm theo ngày thực tế
            var grouped = expanded
                .GroupBy(x => x.Date)
                .Select(g => new DailySummaryDto
                {
                    Date = g.Key,
                    ShiftCount = g.Select(x => x.ShiftId).Distinct().Count(),
                    DoctorCount = g.Select(x => x.DoctorId).Distinct().Count()
                })
                .ToList();

            // Thêm ngày trống
            var fullList = new List<DailySummaryDto>();
            for (int day = 1; day <= endDate.Day; day++)
            {
                var date = new DateOnly(year, month, day);
                var existing = grouped.FirstOrDefault(x => x.Date == date);
                fullList.Add(existing ?? new DailySummaryDto { Date = date, ShiftCount = 0, DoctorCount = 0 });
            }

            return fullList.OrderBy(x => x.Date).ToList();
        }

        public async Task<List<WorkScheduleDto>> GetAllWorkSchedulesAsync(DateOnly? startDate, DateOnly? endDate)
        {
            var query = _context.DoctorShifts
                .Include(ds => ds.Doctor)
                    .ThenInclude(d => d.User)
                .Include(ds => ds.Doctor)
                    .ThenInclude(d => d.Room)
                .Include(ds => ds.Shift)
                .Where(ds=>ds.Status=="Active")
                .Select(ds => new WorkScheduleDto
                {
                    DoctorId = ds.DoctorId,
                    DoctorName = ds.Doctor.User.FullName,
                    Specialty = ds.Doctor.Specialty,
                    ShiftId = ds.ShiftId,
                    ShiftType = ds.Shift.ShiftType,
                    StartTime = ds.Shift.StartTime,
                    EndTime = ds.Shift.EndTime,
                    EffectiveFrom = ds.EffectiveFrom,
                    EffectiveTo = ds.EffectiveTo
                });

            return await query.ToListAsync();
        }
        public async Task<List<DoctorShift>> GetExactRangeAsync(int shiftId, DateOnly fromDate, DateOnly toDate)
        {
            return await _context.DoctorShifts
                .Where(ds => ds.ShiftId == shiftId &&
                             ds.EffectiveFrom == fromDate &&
                             ds.EffectiveTo == toDate)
                .Include(ds => ds.Doctor)
                .ToListAsync();
        }
        public async Task<List<DoctorShift>> GetAllAsync(Expression<Func<DoctorShift, bool>> predicate)
        {
            return await _context.DoctorShifts.Where(predicate).ToListAsync();
        }
        public async Task AddAsync(DoctorShift entity)
        {
            await _context.DoctorShifts.AddAsync(entity);
        }

        public Task UpdateAsync(DoctorShift entity)
        {
            _context.DoctorShifts.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(DoctorShift entity)
        {
            _context.DoctorShifts.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<bool> CheckDoctorShiftLimitAsync(int doctorId, DateOnly date)
        {
            var count = await _context.DoctorShifts
                .AsNoTracking()
                .Where(ds => ds.DoctorId == doctorId &&
                             (ds.Status == null || ds.Status.ToLower() == "active") &&
                             ds.EffectiveFrom <= date &&
                             (ds.EffectiveTo == null || ds.EffectiveTo >= date))
                .Select(ds => ds.ShiftId)
                .Distinct()
                .CountAsync();

            return count < 2;
        }




    }
}
