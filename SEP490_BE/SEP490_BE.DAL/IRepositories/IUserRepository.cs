using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.IRepositories
{
	public interface IUserRepository
	{
		Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<User?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);
        Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);
		Task AddAsync(User user, CancellationToken cancellationToken = default);
        Task UpdateAsync(User user, CancellationToken cancellationToken = default);
        Task<int> GetMaxPatientIdAsync(CancellationToken cancellationToken = default);
	}
}
