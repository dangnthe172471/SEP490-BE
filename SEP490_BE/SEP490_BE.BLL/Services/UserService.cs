using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using System.Linq;

namespace SEP490_BE.BLL.Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepository;

		public UserService(IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			var users = await _userRepository.GetAllAsync(cancellationToken);
			return users.Select(u => new UserDto
			{
				UserId = u.UserId,
				Phone = u.Phone,
				FullName = u.FullName,
				Email = u.Email,
				Role = u.Role?.RoleName,
				Gender = u.Gender,
				Dob = u.Dob
			});
		}

        public async Task<UserDto?> ValidateUserAsync(string phone, string password, CancellationToken cancellationToken = default)
		{
            var user = await _userRepository.GetByPhoneAsync(phone, cancellationToken);
			if (user == null)
			{
				return null;
			}

			var isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
			if (!isValid)
			{
				return null;
			}

			return new UserDto
			{
				UserId = user.UserId,
				Phone = user.Phone,
				FullName = user.FullName,
				Email = user.Email,
				Role = user.Role?.RoleName,
				Gender = user.Gender,
				Dob = user.Dob
			};
		}

        public async Task<int> RegisterAsync(string phone, string password, string fullName, string? email, DateOnly? dob, string? gender, int roleId, CancellationToken cancellationToken = default)
		{
            var existing = await _userRepository.GetByPhoneAsync(phone, cancellationToken);
			if (existing != null)
			{
                throw new InvalidOperationException("Phone already exists.");
			}

			var hashed = BCrypt.Net.BCrypt.HashPassword(password);
			var user = new User
			{
				PasswordHash = hashed,
				FullName = fullName,
				Email = email,
				Phone = phone,
				Dob = dob,
				Gender = gender,
				RoleId = roleId
			};

			// Create Patient record if user is registering as a patient
			if (roleId == 2) // Patient role
			{
				user.Patient = new SEP490_BE.DAL.Models.Patient
				{
					UserId = 0 // Will be set after user is saved
				};
			}

			await _userRepository.AddAsync(user, cancellationToken);
			return user.UserId;
		}

        public async Task<UserDto?> GetUserByPhoneAsync(string phone, CancellationToken cancellationToken = default)
		{
            var user = await _userRepository.GetByPhoneAsync(phone, cancellationToken);
			if (user == null)
			{
				return null;
			}

			var userDto = new UserDto
			{
				UserId = user.UserId,
				Phone = user.Phone,
				FullName = user.FullName,
				Email = user.Email,
				Role = user.Role?.RoleName,
				Gender = user.Gender,
				Dob = user.Dob
			};

        // If user is a patient, include patient-specific information
        if (user.Role?.RoleName == "Patient" && user.Patient != null)
        {
            var patient = user.Patient;
            userDto.Allergies = patient.Allergies;
            userDto.MedicalHistory = patient.MedicalHistory;
        }

			return userDto;
		}

        public async Task<UserDto?> UpdateBasicInfoAsync(int userId, UpdateBasicInfoRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            // Update basic user information
            user.FullName = request.FullName;
            user.Email = request.Email;
            user.Phone = request.Phone;
            user.Dob = !string.IsNullOrEmpty(request.Dob) && DateOnly.TryParse(request.Dob, out var dob) ? dob : null;
            user.Gender = request.Gender;


            await _userRepository.UpdateAsync(user, cancellationToken);

            // Return updated user info directly from the tracked entity
            return new UserDto
            {
                UserId = user.UserId,
                Phone = user.Phone,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role?.RoleName,
                Gender = user.Gender,
                Dob = user.Dob,
                Allergies = user.Patient?.Allergies,
                MedicalHistory = user.Patient?.MedicalHistory
            };
        }

        public async Task<UserDto?> UpdateMedicalInfoAsync(int userId, UpdateMedicalInfoRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            // Check if user is a patient
            if (user.Role?.RoleName != "Patient")
            {
                return null;
            }

            // Create Patient record if it doesn't exist
            if (user.Patient == null)
            {
                // Get next available PatientId
                var maxPatientId = await _userRepository.GetMaxPatientIdAsync(cancellationToken);
                user.Patient = new SEP490_BE.DAL.Models.Patient
                {
                    PatientId = maxPatientId + 1,
                    UserId = user.UserId,
                    Allergies = request.Allergies,
                    MedicalHistory = request.MedicalHistory
                };
            }
            else
            {
                // Update existing patient medical information
                user.Patient.Allergies = request.Allergies;
                user.Patient.MedicalHistory = request.MedicalHistory;
            }

            await _userRepository.UpdateAsync(user, cancellationToken);

            // Return updated user info directly from the tracked entity
            return new UserDto
            {
                UserId = user.UserId,
                Phone = user.Phone,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role?.RoleName,
                Gender = user.Gender,
                Dob = user.Dob,
                Allergies = user.Patient?.Allergies,
                MedicalHistory = user.Patient?.MedicalHistory
            };
        }
	}
}
