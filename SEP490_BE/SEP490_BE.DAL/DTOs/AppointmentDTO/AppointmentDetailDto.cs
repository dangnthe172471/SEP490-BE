namespace SEP490_BE.DAL.DTOs.AppointmentDTO
{
    public class AppointmentDetailDto
    {
        public int AppointmentId { get; set; }

        public string AppointmentDate { get; set; } = default!;

        public string AppointmentTime { get; set; } = default!;

        public string? Status { get; set; }

        public string? CreatedAt { get; set; }

        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = default!;
        public string? DoctorSpecialty { get; set; }

        public int PatientId { get; set; }
        public string PatientName { get; set; } = default!;
        public string? PatientPhone { get; set; }
    }
}
