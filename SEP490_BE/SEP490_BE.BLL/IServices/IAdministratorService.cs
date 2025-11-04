using SEP490_BE.DAL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.IServices
{
    public interface IAdministratorService
    {
        Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<UserDto?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<UserDto?> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
        Task<UserDto?> UpdateUserAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> ToggleUserStatusAsync(int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserDto>> GetAllPatientsAsync(CancellationToken cancellationToken = default);
        Task<SearchUserResponse> SearchUsersAsync(SearchUserRequest request, CancellationToken cancellationToken = default);
    }
}
