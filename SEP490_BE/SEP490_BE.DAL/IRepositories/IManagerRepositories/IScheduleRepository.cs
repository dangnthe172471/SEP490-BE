using SEP490_BE.DAL.DTOs.ManagerDTO.ManagerSchedule;
using SEP490_BE.DAL.DTOs.MedicineDTO;
using SEP490_BE.DAL.Helpers;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.IRepositories.IManagerRepositories
{
    public interface IScheduleRepository
    {
        Task<PaginationHelper.PagedResult<DailyWorkScheduleDto>> GetWorkSchedulesByDateAsync(DateOnly? date, int pageNumber, int pageSize);
        Task<List<DailyWorkScheduleViewDto>> GetWorkScheduleByDateRangeAsync(DateOnly startDate, DateOnly endDate);
        Task<PaginationHelper.PagedResult<WorkScheduleDto>> GetAllSchedulesAsync(int pageNumber, int pageSize);
        Task UpdateWorkScheduleByDateAsync(UpdateWorkScheduleByDateRequest request);
        Task UpdateWorkScheduleByIdAsync(UpdateWorkScheduleByIdRequest request);
        Task<List<DailySummaryDto>> GetMonthlyWorkSummaryAsync(int year, int month);
        Task<List<WorkScheduleDto>> GetAllWorkSchedulesAsync(DateOnly? from = null, DateOnly? to = null);

        #region cap nhat lich theo range
        Task<List<DoctorShift>> GetExactRangeAsync(int shiftId, DateOnly fromDate, DateOnly toDate);
        Task<List<DoctorShift>> GetAllAsync(Expression<Func<DoctorShift, bool>> predicate);
        Task AddAsync(DoctorShift entity);
        Task UpdateAsync(DoctorShift entity);
        Task DeleteAsync(DoctorShift entity);
        Task SaveChangesAsync();
        Task<bool> CheckDoctorShiftLimitAsync(int doctorId, DateOnly date);
        #endregion

    }
}
