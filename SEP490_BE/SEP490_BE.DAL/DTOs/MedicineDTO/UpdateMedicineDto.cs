using System.ComponentModel.DataAnnotations;

namespace SEP490_BE.DAL.DTOs.MedicineDTO
{
    public class UpdateMedicineDto
    {
        [MaxLength(200, ErrorMessage = "MedicineName cannot exceed 200 characters.")]
        public string? MedicineName { get; set; }

        [MaxLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        public string? Status { get; set; }

        [MaxLength(200, ErrorMessage = "ActiveIngredient cannot exceed 200 characters.")]
        public string? ActiveIngredient { get; set; }

        [MaxLength(50, ErrorMessage = "Strength cannot exceed 50 characters.")]
        public string? Strength { get; set; }

        [MaxLength(100, ErrorMessage = "DosageForm cannot exceed 100 characters.")]
        public string? DosageForm { get; set; }

        [MaxLength(50, ErrorMessage = "Route cannot exceed 50 characters.")]
        public string? Route { get; set; }

        [MaxLength(50, ErrorMessage = "PrescriptionUnit cannot exceed 50 characters.")]
        public string? PrescriptionUnit { get; set; }

        [MaxLength(100, ErrorMessage = "TherapeuticClass cannot exceed 100 characters.")]
        public string? TherapeuticClass { get; set; }

        [MaxLength(100, ErrorMessage = "PackSize cannot exceed 100 characters.")]
        public string? PackSize { get; set; }

        public string? CommonSideEffects { get; set; }

        [MaxLength(500, ErrorMessage = "NoteForDoctor cannot exceed 500 characters.")]
        public string? NoteForDoctor { get; set; }
    }
}
