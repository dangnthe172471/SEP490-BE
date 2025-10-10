using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using BCrypt.Net;

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
				Username = u.Username,
				FullName = u.FullName,
				Email = u.Email,
				Phone = u.Phone,
				Role = u.Role?.RoleName,
				Gender = u.Gender,
				Dob = u.Dob
			});
		}

		public async Task<UserDto?> ValidateUserAsync(string username, string password, CancellationToken cancellationToken = default)
		{
			var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
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
				Username = user.Username,
				FullName = user.FullName,
				Email = user.Email,
				Phone = user.Phone,
				Role = user.Role?.RoleName,
				Gender = user.Gender,
				Dob = user.Dob
			};
		}

		public async Task<int> RegisterAsync(string username, string password, string fullName, string? email, string? phone, DateOnly? dob, string? gender, int roleId, CancellationToken cancellationToken = default)
		{
			var existing = await _userRepository.GetByUsernameAsync(username, cancellationToken);
			if (existing != null)
			{
				throw new InvalidOperationException("Username already exists.");
			}

			var hashed = BCrypt.Net.BCrypt.HashPassword(password);
			var user = new User
			{
				Username = username,
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
