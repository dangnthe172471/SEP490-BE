namespace SEP490_BE.DAL.DTOs.MedicineDTO
{
    public class ReadMedicineDto
    {
        public int MedicineId { get; set; }

        public string MedicineName { get; set; } = string.Empty;

        public string? Status { get; set; }

        public int ProviderId { get; set; }
        public string? ProviderName { get; set; }

        public string? ActiveIngredient { get; set; }
        public string? Strength { get; set; }
        public string? DosageForm { get; set; }
        public string? Route { get; set; }
        public string? PrescriptionUnit { get; set; }
        public string? TherapeuticClass { get; set; }
        public string? PackSize { get; set; }
        public string? CommonSideEffects { get; set; }
        public string? NoteForDoctor { get; set; }
    }
}
