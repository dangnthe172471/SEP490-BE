using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IDoctorShiftRepository
    {
        Task<bool> IsShiftConflictAsync(int doctorId, int shiftId, DateOnly from, DateOnly to);
        Task AddDoctorShiftAsync(DoctorShift entity);
        Task<List<DoctorShift>> GetSchedulesAsync(DateOnly from, DateOnly to);
    }
}
