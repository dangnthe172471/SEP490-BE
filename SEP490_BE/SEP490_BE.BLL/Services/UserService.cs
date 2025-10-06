using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;

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
				Role = u.Role,
				Gender = u.Gender,
				Dob = u.Dob
			});
		}
	}
}
