using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.DoctorStatisticsDTO;
using SEP490_BE.DAL.IRepositories;

namespace SEP490_BE.BLL.Services
{
    public class DoctorStatisticsService : IDoctorStatisticsService
    {
        private readonly IDoctorStatisticsRepository _doctorStatisticsRepository;

        public DoctorStatisticsService(IDoctorStatisticsRepository doctorStatisticsRepository)
        {
            _doctorStatisticsRepository = doctorStatisticsRepository;
        }

        public Task<List<DoctorPatientCountDto>> GetPatientCountByDoctorAsync(DateTime fromDate, DateTime toDate)
        {
            return _doctorStatisticsRepository.GetPatientCountByDoctorAsync(fromDate, toDate);
        }

        public Task<List<DoctorVisitTrendPointDto>> GetDoctorVisitTrendAsync(DateTime fromDate, DateTime toDate, int? doctorId = null)
        {
            return _doctorStatisticsRepository.GetDoctorVisitTrendAsync(fromDate, toDate, doctorId);
        }

        public Task<List<DoctorReturnRateDto>> GetDoctorReturnRatesAsync(DateTime fromDate, DateTime toDate)
        {
            return _doctorStatisticsRepository.GetDoctorReturnRatesAsync(fromDate, toDate);
        }

        public async Task<DoctorStatisticsSummaryDto> GetDoctorStatisticsSummaryAsync(DateTime fromDate, DateTime toDate, int? doctorId = null)
        {
            var patientCounts = await _doctorStatisticsRepository.GetPatientCountByDoctorAsync(fromDate, toDate);
            var visitTrend = await _doctorStatisticsRepository.GetDoctorVisitTrendAsync(fromDate, toDate, doctorId);
            var returnRates = await _doctorStatisticsRepository.GetDoctorReturnRatesAsync(fromDate, toDate);

            return new DoctorStatisticsSummaryDto
            {
                PatientCountByDoctor = patientCounts,
                VisitTrend = visitTrend,
                ReturnRates = returnRates
            };
        }
    }
}
