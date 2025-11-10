using SEP490_BE.DAL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.IServices.IDoctorServices
{
    public interface IDoctorScheduleService
    {
        Task<List<DoctorActiveScheduleRangeDto>> GetDoctorActiveScheduleInRangeAsync(int doctorId, DateOnly startDate, DateOnly endDate);
        Task<List<DoctorActiveScheduleRangeDto>> GetAllDoctorSchedulesByRangeAsync(DateOnly startDate, DateOnly endDate);

        Task<List<int>> GetUserIdsByDoctorIdsAsync(List<int> doctorIds);
    }

}
