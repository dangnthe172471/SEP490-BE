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

        public async Task<User?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
		{
			return await _dbContext.Users
				.Include(u => u.Role)
				.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Phone == phone, cancellationToken);
		}

		public async Task AddAsync(User user, CancellationToken cancellationToken = default)
		{
			await _dbContext.Users.AddAsync(user, cancellationToken);
			await _dbContext.SaveChangesAsync(cancellationToken);
		}
	}
}
