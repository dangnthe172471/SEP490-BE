using System.ComponentModel.DataAnnotations;

namespace SEP490_BE.DAL.DTOs.MedicineDTO
{
    public class CreateMedicineDto
    {
        [Required(ErrorMessage = "MedicineName is required.")]
        [MaxLength(200, ErrorMessage = "MedicineName cannot exceed 200 characters.")]
        public string MedicineName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ActiveIngredient is required.")]
        [MaxLength(200, ErrorMessage = "ActiveIngredient cannot exceed 200 characters.")]
        public string ActiveIngredient { get; set; } = string.Empty;

        [Required(ErrorMessage = "Strength is required.")]
        [MaxLength(50, ErrorMessage = "Strength cannot exceed 50 characters.")]
        public string Strength { get; set; } = string.Empty;

        [Required(ErrorMessage = "DosageForm is required.")]
        [MaxLength(100, ErrorMessage = "DosageForm cannot exceed 100 characters.")]
        public string DosageForm { get; set; } = string.Empty;

        [Required(ErrorMessage = "Route is required.")]
        [MaxLength(50, ErrorMessage = "Route cannot exceed 50 characters.")]
        public string Route { get; set; } = string.Empty;

        [Required(ErrorMessage = "PrescriptionUnit is required.")]
        [MaxLength(50, ErrorMessage = "PrescriptionUnit cannot exceed 50 characters.")]
        public string PrescriptionUnit { get; set; } = string.Empty;

        [Required(ErrorMessage = "TherapeuticClass is required.")]
        [MaxLength(100, ErrorMessage = "TherapeuticClass cannot exceed 100 characters.")]
        public string TherapeuticClass { get; set; } = string.Empty;

        [Required(ErrorMessage = "PackSize is required.")]
        [MaxLength(100, ErrorMessage = "PackSize cannot exceed 100 characters.")]
        public string PackSize { get; set; } = string.Empty;

        // text dài, nên không giới hạn cứng bằng attribute
        public string? CommonSideEffects { get; set; }

        [MaxLength(500, ErrorMessage = "NoteForDoctor cannot exceed 500 characters.")]
        public string? NoteForDoctor { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [MaxLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        public string Status { get; set; } = "Providing"; // Providing / Stopped
    }
}
