using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.DTOs.MedicineDTO;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.IServices
{
    public interface IManagerService
    {
        Task<List<ShiftResponseDTO>> GetAllShiftsAsync();
        Task<List<DoctorDTO>> GetAllDoctorsAsync();
        Task<List<DoctorDTO>> SearchDoctorsAsync(string keyword);
        Task<bool> CheckDoctorConflictAsync(int doctorId, int shiftId, DateOnly from, DateOnly to);
        Task<int> CreateScheduleAsync(CreateScheduleRequestDTO dto);
        Task<List<DoctorShift>> GetSchedulesAsync(DateOnly from, DateOnly to);
        Task<PagedResult<WorkScheduleDto>> GetAllSchedulesAsync(int pageNumber, int pageSize);
        Task<List<DailyWorkScheduleViewDto>> GetWorkScheduleByDateRangeAsync(DateOnly startDate, DateOnly endDate);
        Task<PagedResult<DailyWorkScheduleDto>> GetWorkSchedulesByDateAsync(DateOnly? date, int pageNumber, int pageSize);
        Task UpdateWorkScheduleByDateAsync(UpdateWorkScheduleByDateRequest request);
        Task UpdateWorkScheduleByIdAsync(UpdateWorkScheduleByIdRequest request);
    

}
}
