namespace SEP490_BE.DAL.DTOs.DoctorStatisticsDTO
{
    public class DoctorStatisticsSummaryDto
    {
        public List<DoctorPatientCountDto> PatientCountByDoctor { get; set; } = new();
        public List<DoctorVisitTrendPointDto> VisitTrend { get; set; } = new();
        public List<DoctorReturnRateDto> ReturnRates { get; set; } = new();
    }
}
