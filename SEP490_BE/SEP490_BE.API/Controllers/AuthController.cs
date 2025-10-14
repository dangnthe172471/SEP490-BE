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
		private readonly IConfiguration _configuration;

		public AuthController(IUserService userService, IConfiguration configuration)
		{
			_userService = userService;
			_configuration = configuration;
		}

		[HttpPost("login")]
    		public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
		{
            var user = await _userService.ValidateUserAsync(request.Phone, request.Password, cancellationToken);
			if (user == null)
			{
				return Unauthorized();
			}

            var token = GenerateJwtToken(user.Phone ?? string.Empty, user.Role ?? string.Empty);
			return Ok(new { token });
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

        private string GenerateJwtToken(string subject, string role)
		{
			var jwtSection = _configuration.GetSection("Jwt");
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"] ?? string.Empty));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>
			{
                new Claim(JwtRegisteredClaimNames.Sub, subject),
                new Claim(ClaimTypes.Name, subject),
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


