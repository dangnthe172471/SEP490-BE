namespace SEP490_BE.DAL.DTOs.PrescriptionDoctorDTO
{
    public class CreatePrescriptionItem
    {
        public int MedicineId { get; set; }
        public string Dosage { get; set; } = null!;
        public string Duration { get; set; } = null!;

        public string? Instruction { get; set; }
    }
}
