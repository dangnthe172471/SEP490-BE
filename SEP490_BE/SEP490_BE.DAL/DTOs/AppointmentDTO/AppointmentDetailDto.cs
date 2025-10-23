namespace SEP490_BE.DAL.DTOs.AppointmentDTO
{
    public class AppointmentDetailDto
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Doctor
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = default!;
        public string? DoctorSpecialty { get; set; }

        // Patient
        public int PatientId { get; set; }
        public string PatientName { get; set; } = default!;
        public string? PatientPhone { get; set; }
    }
}
