using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IServiceRepository
    {
        Task<List<Service>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Service?> GetByIdAsync(int serviceId, CancellationToken cancellationToken = default);
        Task<(List<Service> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAsync(string serviceName, int? excludeServiceId = null, CancellationToken cancellationToken = default);
        Task AddAsync(Service service, CancellationToken cancellationToken = default);
        Task UpdateAsync(Service service, CancellationToken cancellationToken = default);
        Task DeleteAsync(int serviceId, CancellationToken cancellationToken = default);
    }
}

