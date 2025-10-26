using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.DAL.IRepositories
{
	public interface IUserRepository
	{
		Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<List<User>> GetAllPatientsAsync(CancellationToken cancellationToken = default);

        Task<User?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);
        Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);
        Task AddAsync(User user, CancellationToken cancellationToken = default);
        Task UpdateAsync(User user, CancellationToken cancellationToken = default);
        Task DeleteAsync(int userId, CancellationToken cancellationToken = default);
        Task<int> GetMaxPatientIdAsync(CancellationToken cancellationToken = default);
        Task<(List<User> Users, int TotalCount)> SearchUsersAsync(SearchUserRequest request, CancellationToken cancellationToken = default);
        Task<List<User>> GetAllPatientsAsync(CancellationToken cancellationToken = default);

        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    }
}
