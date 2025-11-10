using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IDoctorRepository
    {
        Task<List<Doctor>> GetAllDoctorsAsync();
        Task<List<Doctor>> SearchDoctorsAsync(string keyword);
        Task<List<DoctorActiveScheduleRangeDto>> GetDoctorActiveScheduleInRangeAsync(int doctorId, DateOnly startDate, DateOnly endDate);
        Task<List<DoctorActiveScheduleRangeDto>> GetAllDoctorSchedulesInRangeAsync(DateOnly startDate, DateOnly endDate);
        Task<List<int>> GetUserIdsByDoctorIdsAsync(List<int> doctorIds);
    }
}
