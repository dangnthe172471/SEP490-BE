namespace SEP490_BE.DAL.DTOs.DoctorStatisticsDTO
{
    public class DoctorPatientCountDto
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public string Specialty { get; set; } = null!;
        public int TotalPatients { get; set; }          // distinct bệnh nhân
        public int TotalAppointments { get; set; }      // tổng lượt khám (completed)
    }
}
