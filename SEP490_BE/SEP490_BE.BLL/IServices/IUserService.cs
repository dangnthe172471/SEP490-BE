using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.BLL.IServices
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<UserDto?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<UserDto?> ValidateUserAsync(string phone, string password, CancellationToken cancellationToken = default);
        Task<int> RegisterAsync(string phone, string password, string fullName, string? email, DateOnly? dob, string? gender, int roleId, CancellationToken cancellationToken = default);
        Task<UserDto?> GetUserByPhoneAsync(string phone, CancellationToken cancellationToken = default);
        Task<UserDto?> UpdateBasicInfoAsync(int userId, UpdateBasicInfoRequest request, CancellationToken cancellationToken = default);
        Task<UserDto?> UpdateMedicalInfoAsync(int userId, UpdateMedicalInfoRequest request, CancellationToken cancellationToken = default);
        Task<UserDto?> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
        Task<UserDto?> UpdateUserAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> ToggleUserStatusAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> UpdatePasswordAsync(int userId, string newPassword, CancellationToken cancellationToken = default);
        Task<SearchUserResponse> SearchUsersAsync(SearchUserRequest request, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserDto>> GetAllPatientsAsync(CancellationToken cancellationToken = default);

        Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);

        Task<bool> ResetPasswordAsync(string email, string newPassword, CancellationToken cancellationToken = default);
        Task<bool> VerifyEmailAsync(string email, CancellationToken cancellationToken = default);

    }
}
