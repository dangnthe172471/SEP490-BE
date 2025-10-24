using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.DTOs.MedicineDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IManagerRepository
    {
        Task<PagedResult<DailyWorkScheduleDto>> GetWorkSchedulesByDateAsync(DateOnly? date, int pageNumber, int pageSize);
        Task<List<DailyWorkScheduleViewDto>> GetWorkScheduleByDateRangeAsync(DateOnly startDate, DateOnly endDate);
        Task<PagedResult<WorkScheduleDto>> GetAllSchedulesAsync(int pageNumber, int pageSize);
        Task UpdateWorkScheduleByDateAsync(UpdateWorkScheduleByDateRequest request);
        Task UpdateWorkScheduleByIdAsync(UpdateWorkScheduleByIdRequest request);
    }
}
