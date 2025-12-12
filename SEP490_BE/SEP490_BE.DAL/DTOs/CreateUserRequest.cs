using System.ComponentModel.DataAnnotations;

namespace SEP490_BE.DAL.DTOs
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string FullName { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        public DateOnly? Dob { get; set; }

        public string? Gender { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public int RoleId { get; set; }

        // Patient specific fields
        public string? Allergies { get; set; }
        public string? MedicalHistory { get; set; }

        // Doctor specific fields
        public string? Specialty { get; set; }
        public int? ExperienceYears { get; set; }
        public int? RoomId { get; set; }
    }
}

