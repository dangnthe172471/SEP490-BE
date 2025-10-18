using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories
{
    public class TestTypeRepository : ITestTypeRepository
    {
        private readonly DiamondHealthContext _dbContext;

        public TestTypeRepository(DiamondHealthContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<TestType>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.TestTypes
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<TestType?> GetByIdAsync(int testTypeId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.TestTypes
                .Include(t => t.TestResults)
                .FirstOrDefaultAsync(t => t.TestTypeId == testTypeId, cancellationToken);
        }

        public async Task<(List<TestType> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.TestTypes.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => t.TestName.Contains(searchTerm) || (t.Description != null && t.Description.Contains(searchTerm)));
            }

            // Get total count before paging
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply paging
            var items = await query
                .AsNoTracking()
                .OrderBy(t => t.TestTypeId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task AddAsync(TestType testType, CancellationToken cancellationToken = default)
        {
            await _dbContext.TestTypes.AddAsync(testType, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(TestType testType, CancellationToken cancellationToken = default)
        {
            _dbContext.TestTypes.Update(testType);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int testTypeId, CancellationToken cancellationToken = default)
        {
            var testType = await _dbContext.TestTypes.FindAsync(new object[] { testTypeId }, cancellationToken: cancellationToken);
            if (testType != null)
            {
                _dbContext.TestTypes.Remove(testType);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}