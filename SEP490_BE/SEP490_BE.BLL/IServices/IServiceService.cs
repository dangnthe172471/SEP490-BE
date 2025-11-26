using SEP490_BE.DAL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.IServices
{
    public interface IServiceService
    {
        Task<IEnumerable<ServiceDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ServiceDto?> GetByIdAsync(int serviceId, CancellationToken cancellationToken = default);
        Task<PagedResponse<ServiceDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<int> CreateAsync(CreateServiceRequest request, CancellationToken cancellationToken = default);
        Task<ServiceDto?> UpdateAsync(int serviceId, UpdateServiceRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int serviceId, CancellationToken cancellationToken = default);
    }
}

