using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.TestReDTO;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.IRepositories
{
    public interface ITestResultRepository
    {
        Task<ReadTestResultDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<ReadTestResultDto>> GetByRecordIdAsync(int recordId, CancellationToken ct = default);

        Task<TestResult> CreateAsync(TestResult entity, CancellationToken ct = default);
        Task UpdateAsync(TestResult entity, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);

        Task<bool> RecordExistsAsync(int recordId, CancellationToken ct = default);
        Task<bool> TestTypeExistsAsync(int testTypeId, CancellationToken ct = default);

        Task<TestResult?> GetEntityByIdAsync(int id, CancellationToken ct = default);

        Task<PagedResult<TestWorklistItemDto>> GetWorklistAsync(
            DateOnly? visitDate,
            string? patientName,
            int pageNumber,
            int pageSize,
            RequiredState requiredState,
            CancellationToken ct = default);

        // Lấy danh sách loại xét nghiệm từ Service(Category="Test")
        Task<List<TestTypeLite>> GetTestTypesAsync(CancellationToken ct = default);

        // Tạo MedicalService tương ứng cho 1 xét nghiệm (Record + Service)
        Task EnsureMedicalServiceForTestAsync(int recordId, int serviceId, CancellationToken ct = default);
    }
}
