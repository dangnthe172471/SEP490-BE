using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.BLL.IServices
{
	public interface IUserService
	{
		Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<UserDto?> ValidateUserAsync(string phone, string password, CancellationToken cancellationToken = default);
        Task<int> RegisterAsync(string phone, string password, string fullName, string? email, DateOnly? dob, string? gender, int roleId, CancellationToken cancellationToken = default);
	}
}
