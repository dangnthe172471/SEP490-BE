using System.ComponentModel.DataAnnotations;

namespace SEP490_BE.DAL.DTOs.MedicineDTO
{
    public class CreateMedicineDto
    {
        [Required, MaxLength(200)]
        public string MedicineName { get; set; } = string.Empty;

        public string? SideEffects { get; set; }

        public string? Status { get; set; } = "Available";
    }
}
