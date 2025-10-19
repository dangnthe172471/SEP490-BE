using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IManagerRepository
    {
        Task<List<WorkScheduleDto>> GetWorkScheduleByDateAsync(DateOnly date);
        Task<List<DailyWorkScheduleDto>> GetWorkScheduleByDateRangeAsync(DateOnly startDate, DateOnly endDate);
    }
}
