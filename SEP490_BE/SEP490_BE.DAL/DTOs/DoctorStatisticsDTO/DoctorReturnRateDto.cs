namespace SEP490_BE.DAL.DTOs.DoctorStatisticsDTO
{
    public class DoctorReturnRateDto
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public int TotalPatients { get; set; }          // distinct bệnh nhân
        public int ReturnPatients { get; set; }         // distinct bệnh nhân có >= 2 lượt
        public double ReturnRate { get; set; }          // %
    }
}
