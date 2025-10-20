using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.DTOs;

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
			
			// If user has a Patient record, handle it properly
			if (user.Patient != null)
			{
				// Check if this is a new Patient record (not tracked by EF)
				var existingPatient = await _dbContext.Patients
					.FirstOrDefaultAsync(p => p.PatientId == user.Patient.PatientId, cancellationToken);
				
				if (existingPatient == null)
				{
					// New Patient record
					_dbContext.Patients.Add(user.Patient);
				}
				else
				{
					// Update existing Patient record
					_dbContext.Patients.Update(user.Patient);
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

        public async Task DeleteAsync(int userId, CancellationToken cancellationToken = default)
		{
			var user = await _dbContext.Users
				.Include(u => u.Patient)
				.Include(u => u.Doctor)
				.Include(u => u.Receptionist)
				.Include(u => u.PharmacyProvider)
				.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

			if (user != null)
			{
                // Guard: prevent deletion when dependent records exist to avoid FK violations
                // Patients: Appointments, ChatLogs
                if (user.Patient != null)
                {
                    var hasPatientAppointments = await _dbContext.Appointments
                        .AnyAsync(a => a.PatientId == user.Patient.PatientId, cancellationToken);
                    var hasPatientChats = await _dbContext.ChatLogs
                        .AnyAsync(c => c.PatientId == user.Patient.PatientId, cancellationToken);

                    if (hasPatientAppointments || hasPatientChats)
                    {
                        throw new InvalidOperationException("Không thể xóa vì bệnh nhân còn dữ liệu liên quan (lịch hẹn hoặc chat).");
                    }
                }

                // Doctors: Appointments, Prescriptions, DoctorShifts, DoctorShiftExchanges
                if (user.Doctor != null)
                {
                    var hasDoctorAppointments = await _dbContext.Appointments
                        .AnyAsync(a => a.DoctorId == user.Doctor.DoctorId, cancellationToken);
                    var hasPrescriptions = await _dbContext.Prescriptions
                        .AnyAsync(p => p.DoctorId == user.Doctor.DoctorId, cancellationToken);
                    var hasDoctorShifts = await _dbContext.DoctorShifts
                        .AnyAsync(s => s.DoctorId == user.Doctor.DoctorId, cancellationToken);
                    var hasShiftExchanges = await _dbContext.DoctorShiftExchanges
                        .AnyAsync(e => e.Doctor1Id == user.Doctor.DoctorId || e.Doctor2Id == user.Doctor.DoctorId, cancellationToken);

                    if (hasDoctorAppointments || hasPrescriptions || hasDoctorShifts || hasShiftExchanges)
                    {
                        throw new InvalidOperationException("Không thể xóa vì bác sĩ còn dữ liệu liên quan (lịch hẹn, đơn thuốc hoặc ca trực).");
                    }
                }

                // Receptionists: Appointments, ChatLogs
                if (user.Receptionist != null)
                {
                    var hasReceptionAppointments = await _dbContext.Appointments
                        .AnyAsync(a => a.ReceptionistId == user.Receptionist.ReceptionistId, cancellationToken);
                    var hasReceptionChats = await _dbContext.ChatLogs
                        .AnyAsync(c => c.ReceptionistId == user.Receptionist.ReceptionistId, cancellationToken);

                    if (hasReceptionAppointments || hasReceptionChats)
                    {
                        throw new InvalidOperationException("Không thể xóa vì lễ tân còn dữ liệu liên quan (lịch hẹn hoặc chat).");
                    }
                }

                // Pharmacy providers: Medicines
                if (user.PharmacyProvider != null)
                {
                    var hasMedicines = await _dbContext.Medicines
                        .AnyAsync(m => m.ProviderId == user.PharmacyProvider.ProviderId, cancellationToken);

                    if (hasMedicines)
                    {
                        throw new InvalidOperationException("Không thể xóa vì nhà thuốc còn dữ liệu liên quan (thuốc).");
                    }
                }

				// Remove related entities first
				if (user.Patient != null)
				{
					_dbContext.Patients.Remove(user.Patient);
				}
				if (user.Doctor != null)
				{
					_dbContext.Doctors.Remove(user.Doctor);
				}
				if (user.Receptionist != null)
				{
					_dbContext.Receptionists.Remove(user.Receptionist);
				}
				if (user.PharmacyProvider != null)
				{
					_dbContext.PharmacyProviders.Remove(user.PharmacyProvider);
				}

				// Remove the user
				_dbContext.Users.Remove(user);
				await _dbContext.SaveChangesAsync(cancellationToken);
			}
		}

        public async Task<(List<User> Users, int TotalCount)> SearchUsersAsync(SearchUserRequest request, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Users
                .Include(u => u.Role)
                .Include(u => u.Patient)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.FullName))
            {
                query = query.Where(u => u.FullName.Contains(request.FullName));
            }

            if (!string.IsNullOrEmpty(request.Phone))
            {
                query = query.Where(u => u.Phone.Contains(request.Phone));
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                query = query.Where(u => u.Email != null && u.Email.Contains(request.Email));
            }

            if (!string.IsNullOrEmpty(request.Role))
            {
                query = query.Where(u => u.Role != null && u.Role.RoleName == request.Role);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == request.IsActive.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var users = await query
                .OrderBy(u => u.FullName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return (users, totalCount);
        }
	}
}
