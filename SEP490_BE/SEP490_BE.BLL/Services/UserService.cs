using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.BLL.Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepository;

		public UserService(IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			var users = await _userRepository.GetAllAsync(cancellationToken);
			return users.Select(u => new UserDto
			{
				UserId = u.UserId,
				Phone = u.Phone,
				FullName = u.FullName,
				Email = u.Email,
				Role = u.Role?.RoleName,
				Gender = u.Gender,
				Dob = u.Dob
			});
		}

        public async Task<UserDto?> ValidateUserAsync(string phone, string password, CancellationToken cancellationToken = default)
		{
            var user = await _userRepository.GetByPhoneAsync(phone, cancellationToken);
			if (user == null)
			{
				return null;
			}

			var isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
			if (!isValid)
			{
				return null;
			}

			return new UserDto
			{
				UserId = user.UserId,
				Phone = user.Phone,
				FullName = user.FullName,
				Email = user.Email,
				Role = user.Role?.RoleName,
				Gender = user.Gender,
				Dob = user.Dob
			};
		}

        public async Task<int> RegisterAsync(string phone, string password, string fullName, string? email, DateOnly? dob, string? gender, int roleId, CancellationToken cancellationToken = default)
		{
            var existing = await _userRepository.GetByPhoneAsync(phone, cancellationToken);
			if (existing != null)
			{
                throw new InvalidOperationException("Phone already exists.");
			}

			var hashed = BCrypt.Net.BCrypt.HashPassword(password);
			var user = new User
			{
				PasswordHash = hashed,
				FullName = fullName,
				Email = email,
				Phone = phone,
				Dob = dob,
				Gender = gender,
				RoleId = roleId
			};

			await _userRepository.AddAsync(user, cancellationToken);
			return user.UserId;
		}
	}
}
