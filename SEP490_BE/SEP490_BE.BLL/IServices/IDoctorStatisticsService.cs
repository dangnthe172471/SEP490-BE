using SEP490_BE.DAL.DTOs.DoctorStatisticsDTO;

namespace SEP490_BE.BLL.IServices
{
    public interface IDoctorStatisticsService
    {
        /// Chart 1: Số lượng bệnh nhân và lượt khám theo từng bác sĩ.
        Task<List<DoctorPatientCountDto>> GetPatientCountByDoctorAsync(DateTime fromDate, DateTime toDate);

        /// Chart 2: Xu hướng số ca khám theo thời gian.
        /// Dùng để theo dõi hoạt động khám chữa bệnh của bác sĩ theo ngày/tuần/tháng.
        Task<List<DoctorVisitTrendPointDto>> GetDoctorVisitTrendAsync(DateTime fromDate, DateTime toDate, int? doctorId = null);

        /// Chart 3: Tỷ lệ bệnh nhân tái khám theo bác sĩ.
        /// Dùng để đánh giá mức độ quay lại/tin tưởng của bệnh nhân.
        Task<List<DoctorReturnRateDto>> GetDoctorReturnRatesAsync(DateTime fromDate, DateTime toDate);

        /// Trả về cả 3 nhóm thống kê trong 1 call cho dashboard.
        Task<DoctorStatisticsSummaryDto> GetDoctorStatisticsSummaryAsync(DateTime fromDate, DateTime toDate, int? doctorId = null);
    }
}
