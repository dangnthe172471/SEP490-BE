using System.ComponentModel.DataAnnotations;

namespace SEP490_BE.DAL.DTOs
{
    public class UpdateUserRequest
    {
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string? Password { get; set; }

        public string? FullName { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        public DateOnly? Dob { get; set; }

        public string? Gender { get; set; }

        public int? RoleId { get; set; }

        public bool? IsActive { get; set; }

        // Patient specific fields
        public string? Allergies { get; set; }
        public string? MedicalHistory { get; set; }
    }
}

