using SEP490_BE.DAL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.IServices
{
    public interface ITestTypeService
    {
        Task<IEnumerable<TestTypeDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TestTypeDto?> GetByIdAsync(int testTypeId, CancellationToken cancellationToken = default);
        Task<PagedResponse<TestTypeDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<int> CreateAsync(CreateTestTypeRequest request, CancellationToken cancellationToken = default);
        Task<TestTypeDto?> UpdateAsync(int testTypeId, UpdateTestTypeRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int testTypeId, CancellationToken cancellationToken = default);
    }
}
