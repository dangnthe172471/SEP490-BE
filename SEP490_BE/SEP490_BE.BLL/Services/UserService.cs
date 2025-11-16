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
                Dob = u.Dob,
                IsActive = u.IsActive,
                EmailVerified = u.EmailVerified,
                Avatar = u.Avatar
            });
        }

        public async Task<UserDto?> ValidateUserAsync(string phone, string password, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByPhoneAsync(phone, cancellationToken);
			if (user == null)
			{
				return null;
			}

			// Check if user is active
			if (!user.IsActive)
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
				Dob = user.Dob,
				IsActive = user.IsActive,
				EmailVerified = user.EmailVerified,
				Avatar = user.Avatar
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
                RoleId = roleId,
                IsActive = true,
                EmailVerified = false // Email chưa được xác thực khi đăng ký
            };

            // Create Patient record if user is registering as a patient
            if (roleId == 2) // Patient role
            {
                // Get next available PatientId
                var maxPatientId = await _userRepository.GetMaxPatientIdAsync(cancellationToken);
                user.Patient = new SEP490_BE.DAL.Models.Patient
                {
                    PatientId = maxPatientId + 1,
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
                Dob = user.Dob,
                IsActive = user.IsActive,
                EmailVerified = user.EmailVerified,
                Avatar = user.Avatar
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
                IsActive = user.IsActive,
                EmailVerified = user.EmailVerified,
                Avatar = user.Avatar,
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
                IsActive = user.IsActive,
                EmailVerified = user.EmailVerified,
                Avatar = user.Avatar,
                Allergies = user.Patient?.Allergies,
                MedicalHistory = user.Patient?.MedicalHistory
            };
        }

        public async Task<UserDto?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
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
                Dob = user.Dob,
                IsActive = user.IsActive,
                EmailVerified = user.EmailVerified,
                Avatar = user.Avatar
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

        public async Task<UserDto?> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            // Check if phone already exists
            var existingUser = await _userRepository.GetByPhoneAsync(request.Phone, cancellationToken);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Số điện thoại đã được sử dụng.");
            }

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create user
            var user = new User
            {
                Phone = request.Phone,
                PasswordHash = hashedPassword,
                FullName = request.FullName,
                Email = request.Email,
                Dob = request.Dob,
                Gender = request.Gender,
                RoleId = request.RoleId,
                IsActive = true
            };

            // Create Patient record if user is a patient
            if (request.RoleId == 2) // 2 is Patient role based on DB.sql
            {
                var maxPatientId = await _userRepository.GetMaxPatientIdAsync(cancellationToken);
                user.Patient = new SEP490_BE.DAL.Models.Patient
                {
                    PatientId = maxPatientId + 1,
                    Allergies = request.Allergies,
                    MedicalHistory = request.MedicalHistory
                };
            }

            await _userRepository.AddAsync(user, cancellationToken);

            // Return the created user
            return await GetByIdAsync(user.UserId, cancellationToken);
        }

        public async Task<UserDto?> UpdateUserAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            // Update basic information if provided
            if (!string.IsNullOrEmpty(request.Phone) && request.Phone != user.Phone)
            {
                // Check if new phone already exists
                var existingUser = await _userRepository.GetByPhoneAsync(request.Phone, cancellationToken);
                if (existingUser != null && existingUser.UserId != userId)
                {
                    throw new InvalidOperationException("Số điện thoại đã được sử dụng.");
                }
                user.Phone = request.Phone;
            }

            if (!string.IsNullOrEmpty(request.FullName))
            {
                user.FullName = request.FullName;
            }

            if (request.Email != null)
            {
                user.Email = request.Email;
            }

            if (request.IsActive != null)
            {
                user.IsActive = (bool)request.IsActive;
            }

            if (request.Dob.HasValue)
            {
                user.Dob = request.Dob.Value;
            }

            if (!string.IsNullOrEmpty(request.Gender))
            {
                user.Gender = request.Gender;
            }

            if (request.RoleId.HasValue)
            {
                user.RoleId = request.RoleId.Value;
            }

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            // Handle Patient specific fields
            if (user.Role?.RoleName == "Patient")
            {
                // Create Patient record if it doesn't exist
                if (user.Patient == null)
                {
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
                    // Update existing patient information
                    if (request.Allergies != null)
                    {
                        user.Patient.Allergies = request.Allergies;
                    }
                    if (request.MedicalHistory != null)
                    {
                        user.Patient.MedicalHistory = request.MedicalHistory;
                    }
                }
            }

            await _userRepository.UpdateAsync(user, cancellationToken);

            // Return updated user info
            return await GetByIdAsync(userId, cancellationToken);
        }

        public async Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return false;
            }

            await _userRepository.DeleteAsync(userId, cancellationToken);
            return true;
        }

        public async Task<bool> ToggleUserStatusAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return false;
            }

            user.IsActive = !user.IsActive;
            await _userRepository.UpdateAsync(user, cancellationToken);
            return true;
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return false;
                }

                // Hash the new password
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.PasswordHash = hashedPassword;

                await _userRepository.UpdateAsync(user, cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<SearchUserResponse> SearchUsersAsync(SearchUserRequest request, CancellationToken cancellationToken = default)
        {
            var (users, totalCount) = await _userRepository.SearchUsersAsync(request, cancellationToken);

            var userDtos = users.Select(u => new UserDto
            {
                UserId = u.UserId,
                Phone = u.Phone,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role?.RoleName,
                Gender = u.Gender,
                Dob = u.Dob,
                IsActive = u.IsActive,
                EmailVerified = u.EmailVerified,
                Avatar = u.Avatar,
                Allergies = u.Patient?.Allergies,
                MedicalHistory = u.Patient?.MedicalHistory
            }).ToList();

            return new SearchUserResponse
            {
                Users = userDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        private static readonly Dictionary<string, (string Token, DateTime Expiry)> _resetTokens = new();
        public async Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null) return null;

            var token = Guid.NewGuid().ToString("N");
            _resetTokens[email] = (token, DateTime.UtcNow.AddMinutes(10));
            return token;
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
        {
            if (!_resetTokens.ContainsKey(email)) return false;

            var (storedToken, expiry) = _resetTokens[email];
            if (storedToken != token || DateTime.UtcNow > expiry)
                return false;

            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepository.UpdateAsync(user, cancellationToken);

            _resetTokens.Remove(email);
            return true;
        }
        public async Task<bool> ResetPasswordAsync(string email, string newPassword, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepository.UpdateAsync(user, cancellationToken);

            return true;
        }

        public async Task<IEnumerable<UserDto>> GetAllPatientsAsync(CancellationToken cancellationToken = default)
        {
            var patients = await _userRepository.GetAllPatientsAsync(cancellationToken);
            return patients.Select(u => new UserDto
            {
                UserId = u.UserId,
                Phone = u.Phone,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role?.RoleName,
                Gender = u.Gender,
                Dob = u.Dob,
                IsActive = u.IsActive,
                EmailVerified = u.EmailVerified,
                Avatar = u.Avatar,
                Allergies = u.Patient?.Allergies,
                MedicalHistory = u.Patient?.MedicalHistory
            });
        }

        public async Task<bool> VerifyEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                return false;
            }

            user.EmailVerified = true;
            await _userRepository.UpdateAsync(user, cancellationToken);
            return true;
        }
    }
}
