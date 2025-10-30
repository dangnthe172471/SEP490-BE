using SEP490_BE.BLL.IServices.IDoctorServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Services.DoctorServices
{
    public class DoctorScheduleService : IDoctorScheduleService
    {
        private readonly IDoctorRepository _repo;

        public DoctorScheduleService(IDoctorRepository repo)
        {
            _repo = repo;
        }
        public async Task<List<DoctorActiveScheduleRangeDto>> GetDoctorActiveScheduleInRangeAsync(int doctorId, DateOnly startDate, DateOnly endDate)
        {
            if (doctorId <= 0)
                throw new ArgumentException("DoctorId không hợp lệ.");

            
            if (startDate == default || endDate == default)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                startDate = new DateOnly(today.Year, today.Month, 1);
                endDate = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            }

            var schedules = await _repo.GetDoctorActiveScheduleInRangeAsync(doctorId, startDate, endDate);

            return schedules ?? new List<DoctorActiveScheduleRangeDto>();
        }
    }
}
