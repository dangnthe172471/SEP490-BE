using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.BLL.IServices
{
	public interface IUserService
	{
		Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
		Task<UserDto?> ValidateUserAsync(string username, string password, CancellationToken cancellationToken = default);
		Task<int> RegisterAsync(string username, string password, string fullName, string? email, string? phone, DateOnly? dob, string? gender, int roleId, CancellationToken cancellationToken = default);
	}
}
