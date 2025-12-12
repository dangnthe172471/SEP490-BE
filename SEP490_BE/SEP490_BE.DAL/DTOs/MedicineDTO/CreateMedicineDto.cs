using System.ComponentModel.DataAnnotations;

namespace SEP490_BE.DAL.DTOs.MedicineDTO
{
    public class CreateMedicineDto
    {
        [Required(ErrorMessage = "Tên thuốc là bắt buộc.")]
        [MaxLength(200, ErrorMessage = "Tên thuốc không được vượt quá 200 ký tự.")]
        public string MedicineName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hoạt chất là bắt buộc.")]
        [MaxLength(200, ErrorMessage = "Hoạt chất không được vượt quá 200 ký tự.")]
        public string ActiveIngredient { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hàm lượng là bắt buộc.")]
        [MaxLength(50, ErrorMessage = "Hàm lượng không được vượt quá 50 ký tự.")]
        public string Strength { get; set; } = string.Empty;

        [Required(ErrorMessage = "Dạng bào chế là bắt buộc.")]
        [MaxLength(100, ErrorMessage = "Dạng bào chế không được vượt quá 100 ký tự.")]
        public string DosageForm { get; set; } = string.Empty;

        [Required(ErrorMessage = "Đường dùng là bắt buộc.")]
        [MaxLength(50, ErrorMessage = "Đường dùng không được vượt quá 50 ký tự.")]
        public string Route { get; set; } = string.Empty;

        [Required(ErrorMessage = "Đơn vị kê đơn là bắt buộc.")]
        [MaxLength(50, ErrorMessage = "Đơn vị kê đơn không được vượt quá 50 ký tự.")]
        public string PrescriptionUnit { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhóm điều trị là bắt buộc.")]
        [MaxLength(100, ErrorMessage = "Nhóm điều trị không được vượt quá 100 ký tự.")]
        public string TherapeuticClass { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quy cách đóng gói là bắt buộc.")]
        [MaxLength(100, ErrorMessage = "Quy cách đóng gói không được vượt quá 100 ký tự.")]
        public string PackSize { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Tác dụng phụ thường gặp không được vượt quá 1000 ký tự.")]
        public string? CommonSideEffects { get; set; }

        [MaxLength(500, ErrorMessage = "Ghi chú cho bác sĩ không được vượt quá 500 ký tự.")]
        public string? NoteForDoctor { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
        [MaxLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự.")]
        public string Status { get; set; } = "Providing"; // Providing / Stopped
    }
}
