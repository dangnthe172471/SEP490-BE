using System;
using System.Threading;
using System.Threading.Tasks;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.Dashboard;
using SEP490_BE.DAL.IRepositories.Dashboard;

namespace SEP490_BE.BLL.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _repo;

        public DashboardService(IDashboardRepository repo)
        {
            _repo = repo;
        }

        public Task<ClinicStatusDto> GetClinicStatusAsync(DateOnly date, CancellationToken cancellationToken = default)
            => _repo.GetClinicStatusAsync(date, cancellationToken);

        public Task<PatientStatisticsDto> GetPatientStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
            => _repo.GetPatientStatisticsAsync(from, to, cancellationToken);
    }
}



