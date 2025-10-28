using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SEP490_BE.BLL.IServices;
using SEP490_BE.BLL.Services;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.IO;

namespace SEP490_BE.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IResetTokenService _resetTokenService;
        private readonly IEmailService _emailService;

        public AuthController(IUserService userService, IConfiguration configuration, IUserRepository userRepository, IResetTokenService resetTokenService, IEmailService emailService)
		{
			_userService = userService;
			_configuration = configuration;
            _userRepository = userRepository;
            _resetTokenService = resetTokenService;
            _emailService = emailService;
        }

		[HttpPost("login")]
    		public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
		{
            // First check if user exists and is active
            var userFromDb = await _userRepository.GetByPhoneAsync(request.Phone, cancellationToken);
            if (userFromDb == null)
            {
                return Unauthorized(new { message = "Số điện thoại hoặc mật khẩu không đúng" });
            }

            if (!userFromDb.IsActive)
            {
                return Unauthorized(new { message = "Tài khoản của bạn hiện đang tạm khóa. Vui lòng liên hệ bộ phận hỗ trợ để được hỗ trợ." });
            }

            // Then validate credentials
            var user = await _userService.ValidateUserAsync(request.Phone, request.Password, cancellationToken);
			if (user == null)
			{
				return Unauthorized(new { message = "Số điện thoại hoặc mật khẩu không đúng" });
			}

            var token = GenerateJwtToken(user.UserId, user.Phone ?? string.Empty, user.Role ?? string.Empty);
			return Ok(new { 
				token,
				user = new {
					userId = user.UserId,
					phone = user.Phone,
					fullName = user.FullName,
					email = user.Email,
					role = user.Role,
					gender = user.Gender,
					dob = user.Dob,
					isActive = user.IsActive
				}
			});
		}

		[HttpPost("register")]
    		public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
		{
			var userId = await _userService.RegisterAsync(
                request.Phone,
				request.Password,
				request.FullName,
				request.Email,
				request.Dob,
				request.Gender,
				request.RoleId,
				cancellationToken
			);

			return Ok(new { userId });
		}

		[HttpGet("profile")]
		[Authorize]
		public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
		{
			var phone = User.FindFirst(ClaimTypes.Name)?.Value;
			if (string.IsNullOrEmpty(phone))
			{
				return Unauthorized();
			}

			var user = await _userService.GetUserByPhoneAsync(phone, cancellationToken);
			if (user == null)
			{
				return NotFound();
			}

			return Ok(new {
				userId = user.UserId,
				phone = user.Phone,
				fullName = user.FullName,
				email = user.Email,
				role = user.Role,
				gender = user.Gender,
				dob = user.Dob,
				allergies = user.Allergies,
				medicalHistory = user.MedicalHistory,
				avatar = user.Avatar
			});
		}

		[HttpPost("change-password")]
		[Authorize]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
			}

			// Validate input
			if (string.IsNullOrEmpty(request.CurrentPassword))
			{
				return BadRequest(new { message = "Vui lòng nhập mật khẩu hiện tại" });
			}

			if (string.IsNullOrEmpty(request.NewPassword))
			{
				return BadRequest(new { message = "Vui lòng nhập mật khẩu mới" });
			}

			if (request.NewPassword.Length < 6)
			{
				return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự" });
			}

			if (request.CurrentPassword == request.NewPassword)
			{
				return BadRequest(new { message = "Mật khẩu mới phải khác mật khẩu hiện tại" });
			}

			try
			{
				// Get user from database
				var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
				if (user == null)
				{
					return NotFound(new { message = "Không tìm thấy người dùng" });
				}

				// Verify current password
				var isValidCurrentPassword = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
				if (!isValidCurrentPassword)
				{
					return Unauthorized(new { message = "Mật khẩu hiện tại không đúng" });
				}

				// Update password
				var success = await _userService.UpdatePasswordAsync(userId, request.NewPassword, cancellationToken);
				if (!success)
				{
					return BadRequest(new { message = "Không thể cập nhật mật khẩu" });
				}

				return Ok(new { message = "Đổi mật khẩu thành công" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Có lỗi xảy ra khi đổi mật khẩu", error = ex.Message });
			}
		}

		[HttpPost("change-avatar")]
		[Authorize]
		public async Task<IActionResult> ChangeAvatar(IFormFile avatar, CancellationToken cancellationToken)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
			}

			if (avatar == null || avatar.Length == 0)
			{
				return BadRequest(new { message = "Vui lòng chọn ảnh đại diện" });
			}

			// Validate file type
			var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
			if (!allowedTypes.Contains(avatar.ContentType.ToLower()))
			{
				return BadRequest(new { message = "Chỉ chấp nhận file ảnh (JPG, PNG, GIF)" });
			}

			// Validate file size (5MB max)
			if (avatar.Length > 5 * 1024 * 1024)
			{
				return BadRequest(new { message = "Kích thước file không được vượt quá 5MB" });
			}

			try
			{
				// Generate unique filename
				var fileExtension = Path.GetExtension(avatar.FileName);
				var fileName = $"avatar_{userId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
				
				// Create uploads directory if it doesn't exist
				var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
				if (!Directory.Exists(uploadsPath))
				{
					Directory.CreateDirectory(uploadsPath);
				}

				var filePath = Path.Combine(uploadsPath, fileName);

				// Save file
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await avatar.CopyToAsync(stream, cancellationToken);
				}

				// Generate URL for the avatar
				var avatarUrl = $"/uploads/avatars/{fileName}";

				// Update user avatar in database
				var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
				if (user != null)
				{
					// Delete old avatar if exists
					if (!string.IsNullOrEmpty(user.Avatar) && user.Avatar.StartsWith("/uploads/avatars/"))
					{
						var oldAvatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.Avatar.TrimStart('/'));
						if (System.IO.File.Exists(oldAvatarPath))
						{
							System.IO.File.Delete(oldAvatarPath);
						}
					}

					user.Avatar = avatarUrl;
					await _userRepository.UpdateAsync(user, cancellationToken);
				}

				return Ok(new { 
					message = "Cập nhật ảnh đại diện thành công",
					avatarUrl = avatarUrl
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Có lỗi xảy ra khi cập nhật ảnh đại diện", error = ex.Message });
			}
		}

		[HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordByEmail([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest(new { message = "Vui lòng nhập email." });

            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
                return Ok(new { message = "Nếu email tồn tại, hướng dẫn đặt lại mật khẩu đã được gửi." });

            // Sinh mã OTP
            var otpCode = new Random().Next(100000, 999999).ToString();

            // Lưu mã OTP tạm (hoặc dùng Redis/Database)
            await _resetTokenService.StoreOtpAsync(request.Email, otpCode, TimeSpan.FromMinutes(5));

            // Gửi email
            string subject = "Xác thực đặt lại mật khẩu";
            string body = $@"
        <h3>Xin chào {user.FullName},</h3>
        <p>Mã OTP để đặt lại mật khẩu của bạn là:</p>
        <h2 style='color:#007bff'>{otpCode}</h2>
        <p>Mã này sẽ hết hạn sau 5 phút.</p>";

            await _emailService.SendEmailAsync(request.Email, subject, body, cancellationToken);

            return Ok(new { message = "OTP đã được gửi qua email." });
        }

        [HttpPost("verify-email-otp")]
        public async Task<IActionResult> VerifyEmailOtp([FromBody] VerifyOtpRequest request)
        {
            var isValid = await _resetTokenService.ValidateOtpAsync(request.Email, request.OtpCode);
            if (!isValid)
                return BadRequest(new { message = "Mã OTP không hợp lệ hoặc đã hết hạn." });

            var resetToken = await _resetTokenService.GenerateAndStoreTokenAsync(request.Email);
            return Ok(new { message = "Xác thực thành công.", resetToken });
        }

        private string GenerateJwtToken(int userId, string subject, string role)
		{
			var jwtSection = _configuration.GetSection("Jwt");
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"] ?? string.Empty));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>
			{
                new Claim(JwtRegisteredClaimNames.Sub, subject ?? string.Empty), 
				new Claim(ClaimTypes.Name,             subject ?? string.Empty),
				new Claim(ClaimTypes.NameIdentifier,   userId.ToString()),
				new Claim(ClaimTypes.Role,             role ?? string.Empty)
            };

			var token = new JwtSecurityToken(
				issuer: jwtSection["Issuer"],
				audience: jwtSection["Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(int.TryParse(jwtSection["ExpireMinutes"], out var m) ? m : 60),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            bool isValid = await _resetTokenService.ValidateTokenAsync(request.Email, request.Token);
            if (!isValid)
                return BadRequest(new { message = "Token không hợp lệ hoặc đã hết hạn." });

            bool success = await _userService.ResetPasswordAsync(request.Email, request.NewPassword, cancellationToken);
            if (!success)
                return BadRequest(new { message = "Không tìm thấy người dùng hoặc cập nhật thất bại." });

            return Ok(new { message = "Đặt lại mật khẩu thành công." });
        }

    }
}