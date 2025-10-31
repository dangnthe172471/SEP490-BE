using System;
using System.Threading;
using System.Threading.Tasks;
using SEP490_BE.DAL.DTOs.Dashboard;

namespace SEP490_BE.BLL.IServices
{
    public interface IDashboardService
    {
        Task<ClinicStatusDto> GetClinicStatusAsync(DateOnly date, CancellationToken cancellationToken = default);
        Task<PatientStatisticsDto> GetPatientStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    }
}


