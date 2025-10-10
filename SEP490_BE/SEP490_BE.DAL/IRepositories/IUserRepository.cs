using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.IRepositories
{
	public interface IUserRepository
	{
		Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
		Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
		Task AddAsync(User user, CancellationToken cancellationToken = default);
	}
}
