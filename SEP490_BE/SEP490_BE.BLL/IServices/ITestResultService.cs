using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.TestResults;

namespace SEP490_BE.BLL.IServices
{
    public interface ITestResultService
    {
        Task<ReadTestResultDto> CreateAsync(CreateTestResultDto dto, CancellationToken ct = default);
        Task<ReadTestResultDto> UpdateAsync(int testResultId, UpdateTestResultDto dto, CancellationToken ct = default);
        Task DeleteAsync(int testResultId, CancellationToken ct = default);
        Task<List<ReadTestResultDto>> GetByRecordIdAsync(int recordId, CancellationToken ct = default);
        Task<ReadTestResultDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<PagedResult<TestWorklistItemDto>> GetWorklistAsync(TestWorklistQueryDto query, CancellationToken ct = default);
        Task<List<(int TestTypeId, string TestName)>> GetTestTypesAsync(CancellationToken ct = default);
    }
}
