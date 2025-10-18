using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.IRepositories
{
    public interface ITestTypeRepository
    {
        Task<List<TestType>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TestType?> GetByIdAsync(int testTypeId, CancellationToken cancellationToken = default);
        Task<(List<TestType> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);
        Task AddAsync(TestType testType, CancellationToken cancellationToken = default);
        Task UpdateAsync(TestType testType, CancellationToken cancellationToken = default);
        Task DeleteAsync(int testTypeId, CancellationToken cancellationToken = default);
    }
}
