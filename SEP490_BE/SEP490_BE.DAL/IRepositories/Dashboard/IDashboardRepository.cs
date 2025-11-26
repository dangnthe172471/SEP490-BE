using System;
using System.Threading;
using System.Threading.Tasks;
using SEP490_BE.DAL.DTOs.Dashboard;

namespace SEP490_BE.DAL.IRepositories.Dashboard
{
    public interface IDashboardRepository
    {
        Task<ClinicStatusDto> GetClinicStatusAsync(DateOnly date, CancellationToken cancellationToken = default);
        Task<PatientStatisticsDto> GetPatientStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
        Task<TestDiagnosticStatsDto> GetTestDiagnosticStatsAsync(DateOnly from, DateOnly to, string groupBy, CancellationToken cancellationToken = default);
    }
}



