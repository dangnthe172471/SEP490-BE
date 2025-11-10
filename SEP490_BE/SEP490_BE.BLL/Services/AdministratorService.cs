using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Services
{
    public class AdministratorService : IAdministratorService
    {
        private readonly IAdministratorRepository _administratorRepository;

        public AdministratorService(IAdministratorRepository administratorRepository)
        {
            _administratorRepository = administratorRepository;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var users = await _administratorRepository.GetAllAsync(cancellationToken);
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
                Avatar = u.Avatar
            });
        }
        public async Task<UserDto?> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            // Check if phone already exists
            var existingUser = await _administratorRepository.GetByPhoneAsync(request.Phone, cancellationToken);
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
                var maxPatientId = await _administratorRepository.GetMaxPatientIdAsync(cancellationToken);
                user.Patient = new SEP490_BE.DAL.Models.Patient
                {
                    PatientId = maxPatientId + 1,
                    Allergies = request.Allergies,
                    MedicalHistory = request.MedicalHistory
                };
            }

            await _administratorRepository.AddAsync(user, cancellationToken);

            // Return the created user
            return await GetByIdAsync(user.UserId, cancellationToken);
        }

        public async Task<UserDto?> UpdateUserAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _administratorRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            // Update basic information if provided
            if (!string.IsNullOrEmpty(request.Phone) && request.Phone != user.Phone)
            {
                // Check if new phone already exists
                var existingUser = await _administratorRepository.GetByPhoneAsync(request.Phone, cancellationToken);
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
                // Update Role object to match new RoleId for subsequent checks
                if (user.Role != null)
                {
                    user.Role.RoleId = request.RoleId.Value;
                    // Update RoleName based on RoleId (common roles: 1=Admin, 2=Patient, 3=Doctor, 4=Nurse, 5=Receptionist, 6=Pharmacy Provider)
                    user.Role.RoleName = request.RoleId.Value switch
                    {
                        1 => "Administrator",
                        2 => "Patient",
                        3 => "Doctor",
                        4 => "Nurse",
                        5 => "Receptionist",
                        6 => "Pharmacy Provider",
                        _ => user.Role.RoleName
                    };
                }
            }

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            // Handle Patient specific fields
            if (user.Role?.RoleName == "Patient" || user.RoleId == 2)
            {
                // Create Patient record if it doesn't exist
                if (user.Patient == null)
                {
                    var maxPatientId = await _administratorRepository.GetMaxPatientIdAsync(cancellationToken);
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

            await _administratorRepository.UpdateAsync(user, cancellationToken);

            // Return updated user info
            return await GetByIdAsync(userId, cancellationToken);
        }

        public async Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _administratorRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return false;
            }

            await _administratorRepository.DeleteAsync(userId, cancellationToken);
            return true;
        }

        public async Task<bool> ToggleUserStatusAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _administratorRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return false;
            }

            user.IsActive = !user.IsActive;
            await _administratorRepository.UpdateAsync(user, cancellationToken);
            return true;
        }

        public async Task<IEnumerable<UserDto>> GetAllPatientsAsync(CancellationToken cancellationToken = default)
        {
            var patients = await _administratorRepository.GetAllPatientsAsync(cancellationToken);
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
                Avatar = u.Avatar,
                Allergies = u.Patient?.Allergies,
                MedicalHistory = u.Patient?.MedicalHistory
            });
        }

        public async Task<UserDto?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _administratorRepository.GetByIdAsync(userId, cancellationToken);
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

        public async Task<SearchUserResponse> SearchUsersAsync(SearchUserRequest request, CancellationToken cancellationToken = default)
        {
            var (users, totalCount) = await _administratorRepository.SearchUsersAsync(request, cancellationToken);

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


    }
}
