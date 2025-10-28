namespace SEP490_BE.DAL.DTOs.PrescriptionDoctorDTO
{
    public class PrescriptionLineDto
    {
        public int PrescriptionDetailId { get; set; }
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = default!;
        public string Dosage { get; set; } = default!;
        public string Duration { get; set; } = default!;

        public int ProviderId { get; set; }
        public string ProviderName { get; set; } = default!;
        public string? ProviderContact { get; set; }
    }
}
