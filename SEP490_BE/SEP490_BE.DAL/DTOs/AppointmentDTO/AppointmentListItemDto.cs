namespace SEP490_BE.DAL.DTOs.AppointmentDTO
{
    public class AppointmentListItemDto
    {
        public int AppointmentId { get; set; }

        public string AppointmentDate { get; set; } = default!;

        public string AppointmentTime { get; set; } = default!;

        public string? Status { get; set; }

        public int PatientId { get; set; }
        public string PatientName { get; set; } = default!;
        public string? PatientPhone { get; set; }

        public string? ReasonForVisit { get; set; }
    }
}
