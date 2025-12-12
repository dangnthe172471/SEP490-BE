using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using System.Linq;

namespace SEP490_BE.BLL.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<IEnumerable<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);
        return roles.Select(r => new RoleDto
        {
            RoleId = r.RoleId,
            RoleName = r.RoleName
        });
    }
}

