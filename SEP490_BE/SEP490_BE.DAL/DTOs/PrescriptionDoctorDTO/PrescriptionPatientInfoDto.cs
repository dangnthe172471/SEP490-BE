namespace SEP490_BE.DAL.DTOs.PrescriptionDoctorDTO
{
    public class PrescriptionPatientInfoDto
    {
        public int PatientId { get; set; }
        public string Name { get; set; } = default!;
        public string? Gender { get; set; }
        public string? Dob { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
