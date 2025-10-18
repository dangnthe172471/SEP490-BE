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
				.Include(u => u.Patient)
				.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Phone == phone, cancellationToken);
		}

        public async Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
		{
			return await _dbContext.Users
				.Include(u => u.Role)
				.Include(u => u.Patient)
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
		}

		public async Task AddAsync(User user, CancellationToken cancellationToken = default)
		{
			await _dbContext.Users.AddAsync(user, cancellationToken);
			await _dbContext.SaveChangesAsync(cancellationToken);
			
			// Set UserId for Patient record if it exists
			if (user.Patient != null)
			{
				user.Patient.UserId = user.UserId;
				await _dbContext.SaveChangesAsync(cancellationToken);
			}
		}

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
		{
			_dbContext.Users.Update(user);
			
			// If user has a new Patient record, add it to the context
			if (user.Patient != null)
			{
				// Check if this is a new Patient record (not tracked by EF)
				var existingPatient = await _dbContext.Patients
					.FirstOrDefaultAsync(p => p.PatientId == user.Patient.PatientId, cancellationToken);
				
				if (existingPatient == null)
				{
					_dbContext.Patients.Add(user.Patient);
				}
			}
			
			await _dbContext.SaveChangesAsync(cancellationToken);
		}

        public async Task<int> GetMaxPatientIdAsync(CancellationToken cancellationToken = default)
		{
			var maxId = await _dbContext.Patients
				.MaxAsync(p => (int?)p.PatientId, cancellationToken);
			return maxId ?? 0;
		}
	}
}
