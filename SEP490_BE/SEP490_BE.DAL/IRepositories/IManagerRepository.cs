using SEP490_BE.DAL.DTOs.ManageReceptionist.ManagerSchedule;
using SEP490_BE.DAL.DTOs.MedicineDTO;
using SEP490_BE.DAL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IManagerRepository
    {
        Task<PaginationHelper.PagedResult<DailyWorkScheduleDto>> GetWorkSchedulesByDateAsync(DateOnly? date, int pageNumber, int pageSize);
        Task<List<DailyWorkScheduleViewDto>> GetWorkScheduleByDateRangeAsync(DateOnly startDate, DateOnly endDate);
        Task<PaginationHelper.PagedResult<WorkScheduleDto>> GetAllSchedulesAsync(int pageNumber, int pageSize);
        Task UpdateWorkScheduleByDateAsync(UpdateWorkScheduleByDateRequest request);
        Task UpdateWorkScheduleByIdAsync(UpdateWorkScheduleByIdRequest request);
        Task<List<DailySummaryDto>> GetMonthlyWorkSummaryAsync(int year, int month);
        Task<List<WorkScheduleDto>> GetAllWorkSchedulesAsync(DateOnly? from = null, DateOnly? to = null);

    }
}
