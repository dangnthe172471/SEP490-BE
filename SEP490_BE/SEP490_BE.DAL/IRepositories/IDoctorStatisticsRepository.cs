using SEP490_BE.DAL.DTOs.DoctorStatisticsDTO;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IDoctorStatisticsRepository
    {
        Task<List<DoctorPatientCountDto>> GetPatientCountByDoctorAsync(
            DateTime from,
            DateTime to);

        Task<List<DoctorVisitTrendPointDto>> GetDoctorVisitTrendAsync(
            DateTime from,
            DateTime to,
            int? doctorId = null);

        Task<List<DoctorReturnRateDto>> GetDoctorReturnRatesAsync(
            DateTime from,
            DateTime to);
    }
}
