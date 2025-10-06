using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.BLL.IServices
{
	public interface IUserService
	{
		Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
	}
}
