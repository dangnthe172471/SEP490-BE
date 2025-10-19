using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Services
{
    public class TestTypeService : ITestTypeService
    {
        private readonly ITestTypeRepository _testTypeRepository;

        public TestTypeService(ITestTypeRepository testTypeRepository)
        {
            _testTypeRepository = testTypeRepository;
        }

        public async Task<IEnumerable<TestTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var testTypes = await _testTypeRepository.GetAllAsync(cancellationToken);
            return testTypes.Select(t => new TestTypeDto
            {
                TestTypeId = t.TestTypeId,
                TestName = t.TestName,
                Description = t.Description
            });
        }

        public async Task<TestTypeDto?> GetByIdAsync(int testTypeId, CancellationToken cancellationToken = default)
        {
            var testType = await _testTypeRepository.GetByIdAsync(testTypeId, cancellationToken);
            if (testType == null)
            {
                return null;
            }

            return new TestTypeDto
            {
                TestTypeId = testType.TestTypeId,
                TestName = testType.TestName,
                Description = testType.Description
            };
        }

        public async Task<PagedResponse<TestTypeDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var (items, totalCount) = await _testTypeRepository.GetPagedAsync(pageNumber, pageSize, searchTerm, cancellationToken);

            return new PagedResponse<TestTypeDto>
            {
                Items = items.Select(t => new TestTypeDto
                {
                    TestTypeId = t.TestTypeId,
                    TestName = t.TestName,
                    Description = t.Description
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<int> CreateAsync(CreateTestTypeRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.TestName))
            {
                throw new ArgumentException("Test name is required.");
            }

            var testType = new TestType
            {
                TestName = request.TestName.Trim(),
                Description = request.Description?.Trim()
            };

            await _testTypeRepository.AddAsync(testType, cancellationToken);
            return testType.TestTypeId;
        }

        public async Task<TestTypeDto?> UpdateAsync(int testTypeId, UpdateTestTypeRequest request, CancellationToken cancellationToken = default)
        {
            var testType = await _testTypeRepository.GetByIdAsync(testTypeId, cancellationToken);
            if (testType == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(request.TestName))
            {
                throw new ArgumentException("Test name is required.");
            }

            testType.TestName = request.TestName.Trim();
            testType.Description = request.Description?.Trim();

            await _testTypeRepository.UpdateAsync(testType, cancellationToken);

            return new TestTypeDto
            {
                TestTypeId = testType.TestTypeId,
                TestName = testType.TestName,
                Description = testType.Description
            };
        }

        public async Task<bool> DeleteAsync(int testTypeId, CancellationToken cancellationToken = default)
        {
            var testType = await _testTypeRepository.GetByIdAsync(testTypeId, cancellationToken);
            if (testType == null)
            {
                return false;
            }

            await _testTypeRepository.DeleteAsync(testTypeId, cancellationToken);
            return true;
        }
    }
}
