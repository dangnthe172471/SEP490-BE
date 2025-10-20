using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SEP490_BE.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly IOtpService _otpService;
		private readonly IConfiguration _configuration;

		public AuthController(IUserService userService, IOtpService otpService, IConfiguration configuration)
		{
			_userService = userService;
			_otpService = otpService;
			_configuration = configuration;
		}

		[HttpPost("login")]
    		public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
		{
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
					dob = user.Dob
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
				medicalHistory = user.MedicalHistory
			});
		}

		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
		{
			try
			{
				// Check if user exists
				var user = await _userService.GetUserByPhoneAsync(request.Phone, cancellationToken);
				if (user == null)
				{
					return BadRequest(new { message = "Số điện thoại không tồn tại trong hệ thống" });
				}

				// Generate and send OTP
				await _otpService.GenerateOtpAsync(request.Phone, "forgot_password", cancellationToken);

				return Ok(new { message = "Mã OTP đã được gửi đến số điện thoại của bạn" });
			}
			catch (Exception)
			{
				return StatusCode(500, new { message = "Có lỗi xảy ra khi gửi mã OTP" });
			}
		}

		[HttpPost("verify-otp")]
		public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request, CancellationToken cancellationToken)
		{
			try
			{
				var isValid = await _otpService.VerifyOtpAsync(request.Phone, request.OtpCode, "forgot_password", cancellationToken);
				
				if (!isValid)
				{
					return BadRequest(new { message = "Mã OTP không đúng hoặc đã hết hạn" });
				}

				return Ok(new { message = "Mã OTP hợp lệ" });
			}
			catch (Exception)
			{
				return StatusCode(500, new { message = "Có lỗi xảy ra khi xác thực mã OTP" });
			}
		}

		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
		{
			try
			{
				// Verify OTP first
				var isOtpValid = await _otpService.IsOtpValidAsync(request.Phone, request.OtpCode, "forgot_password", cancellationToken);
				if (!isOtpValid)
				{
					return BadRequest(new { message = "Mã OTP không đúng hoặc đã hết hạn" });
				}

				// Get user
				var user = await _userService.GetUserByPhoneAsync(request.Phone, cancellationToken);
				if (user == null)
				{
					return BadRequest(new { message = "Số điện thoại không tồn tại trong hệ thống" });
				}

				// Update password
				var success = await _userService.UpdatePasswordAsync(user.UserId, request.NewPassword, cancellationToken);
				if (!success)
				{
					return StatusCode(500, new { message = "Có lỗi xảy ra khi cập nhật mật khẩu" });
				}

				// Mark OTP as used
				await _otpService.VerifyOtpAsync(request.Phone, request.OtpCode, "forgot_password", cancellationToken);

				return Ok(new { message = "Mật khẩu đã được cập nhật thành công" });
			}
			catch (Exception)
			{
				return StatusCode(500, new { message = "Có lỗi xảy ra khi đặt lại mật khẩu" });
			}
		}

        private string GenerateJwtToken(int userId, string subject, string role)
		{
			var jwtSection = _configuration.GetSection("Jwt");
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"] ?? string.Empty));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>
			{
                new Claim(JwtRegisteredClaimNames.Sub, subject),
                new Claim(ClaimTypes.Name, subject),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
				new Claim(ClaimTypes.Role, role)
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
	}
}