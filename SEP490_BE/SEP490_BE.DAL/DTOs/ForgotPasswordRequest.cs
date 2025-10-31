using System.ComponentModel.DataAnnotations;

namespace SEP490_BE.DAL.DTOs
{
    public class ForgotPasswordRequest
    {
        //[Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        //[Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        //public string Phone { get; set; } = null!;

        public string Email { get; set; } = string.Empty;
    }

    public class VerifyOtpRequest
    {
        //[Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        //public string Phone { get; set; } = null!;

        //[Required(ErrorMessage = "Mã OTP là bắt buộc")]
        //[StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 chữ số")]
        //public string OtpCode { get; set; } = null!;
        public string Email { get; set; }
        public string OtpCode { get; set; }
    }

    public class ResetPasswordRequest
    {
        //[Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        //public string Phone { get; set; } = null!;

        //[Required(ErrorMessage = "Mã OTP là bắt buộc")]
        //[StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 chữ số")]
        //public string OtpCode { get; set; } = null!;

        //[Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        //[StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        //public string NewPassword { get; set; } = null!;

        //public string Phone { get; set; } = string.Empty;
        //public string ResetToken { get; set; } = string.Empty;
        //public string NewPassword { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}

