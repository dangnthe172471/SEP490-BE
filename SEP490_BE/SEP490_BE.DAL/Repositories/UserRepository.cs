using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly DiamondHealthContext _dbContext;

		public UserRepository(DiamondHealthContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			return await _dbContext.Users
				.Include(u => u.Role)
				.AsNoTracking()
				.ToListAsync(cancellationToken);
		}
	}
}
