namespace SEP490_BE.DAL.DTOs.PrescriptionDoctorDTO
{
    public class PrescriptionDoctorInfoDto
    {
        public int DoctorId { get; set; }
        public string Name { get; set; } = default!;
        public string? Specialty { get; set; }
        public string? Phone { get; set; }
    }
}
