namespace SEP490_BE.DAL.DTOs.DoctorStatisticsDTO
{
    public class DoctorVisitTrendPointDto
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public DateTime Date { get; set; }              // ngày (hoặc mốc time group)
        public int VisitCount { get; set; }             // số ca completed trong ngày đó
    }
}
