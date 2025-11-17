namespace SEP490_BE.DAL.DTOs.PrescriptionDoctorDTO
{
    public class PrescriptionLineDto
    {
        public int PrescriptionDetailId { get; set; }

        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = null!;

        public string? ActiveIngredient { get; set; }
        public string? Strength { get; set; }
        public string? DosageForm { get; set; }
        public string? Route { get; set; }
        public string? PrescriptionUnit { get; set; }
        public string? TherapeuticClass { get; set; }
        public string? PackSize { get; set; }
        public string? CommonSideEffects { get; set; }
        public string? NoteForDoctor { get; set; }

        public string Dosage { get; set; } = null!;
        public string Duration { get; set; } = null!;

        public string? Instruction { get; set; }

        public int? ProviderId { get; set; }
        public string? ProviderName { get; set; }
        public string? ProviderContact { get; set; }
    }
}
