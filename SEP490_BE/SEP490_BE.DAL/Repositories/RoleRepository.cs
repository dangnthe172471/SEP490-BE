using Microsoft.EntityFrameworkCore;
using System.Linq;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly DiamondHealthContext _dbContext;

    public RoleRepository(DiamondHealthContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .OrderBy(r => r.RoleName)
            .ToListAsync(cancellationToken);
    }
}

