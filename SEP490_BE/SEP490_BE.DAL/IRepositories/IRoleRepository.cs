using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.IRepositories;

public interface IRoleRepository
{
    Task<List<Role>> GetAllAsync(CancellationToken cancellationToken = default);
}

