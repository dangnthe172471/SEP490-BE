using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.BLL.IServices;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default);
}

