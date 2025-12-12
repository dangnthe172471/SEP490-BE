using System.ComponentModel.DataAnnotations;

namespace SEP490_BE.DAL.DTOs.MedicineDTO
{
    public class UpdateMedicineDto
    {
        [MaxLength(200, ErrorMessage = "Tên thuốc không được vượt quá 200 ký tự.")]
        public string? MedicineName { get; set; }

        [MaxLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự.")]
        public string? Status { get; set; }

        [MaxLength(200, ErrorMessage = "Hoạt chất không được vượt quá 200 ký tự.")]
        public string? ActiveIngredient { get; set; }

        [MaxLength(50, ErrorMessage = "Hàm lượng không được vượt quá 50 ký tự.")]
        public string? Strength { get; set; }

        [MaxLength(100, ErrorMessage = "Dạng bào chế không được vượt quá 100 ký tự.")]
        public string? DosageForm { get; set; }

        [MaxLength(50, ErrorMessage = "Đường dùng không được vượt quá 50 ký tự.")]
        public string? Route { get; set; }

        [MaxLength(50, ErrorMessage = "Đơn vị kê đơn không được vượt quá 50 ký tự.")]
        public string? PrescriptionUnit { get; set; }

        [MaxLength(100, ErrorMessage = "Nhóm điều trị không được vượt quá 100 ký tự.")]
        public string? TherapeuticClass { get; set; }

        [MaxLength(100, ErrorMessage = "Quy cách đóng gói không được vượt quá 100 ký tự.")]
        public string? PackSize { get; set; }

        [MaxLength(1000, ErrorMessage = "Tác dụng phụ thường gặp không được vượt quá 1000 ký tự.")]
        public string? CommonSideEffects { get; set; }

        [MaxLength(500, ErrorMessage = "Ghi chú cho bác sĩ không được vượt quá 500 ký tự.")]
        public string? NoteForDoctor { get; set; }
    }
}
