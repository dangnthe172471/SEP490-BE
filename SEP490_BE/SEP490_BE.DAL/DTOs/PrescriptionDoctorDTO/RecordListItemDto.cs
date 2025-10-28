namespace SEP490_BE.DAL.DTOs.PrescriptionDoctorDTO
{
    public class RecordListItemDto
    {
        public int RecordId { get; set; }
        public int AppointmentId { get; set; }
        public DateTime VisitAt { get; set; }

        public int PatientId { get; set; }
        public string PatientName { get; set; } = default!;
        public string? Gender { get; set; }
        public DateOnly? Dob { get; set; }
        public string? Phone { get; set; }

        public string? DiagnosisRaw { get; set; }

        public bool HasPrescription { get; set; }
        public int? LatestPrescriptionId { get; set; }
    }
}
