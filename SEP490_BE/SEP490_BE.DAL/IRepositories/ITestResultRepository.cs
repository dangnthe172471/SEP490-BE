using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.TestReDTO;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.IRepositories
{
    public interface ITestResultRepository
    {
        Task<TestResult?> GetEntityByIdAsync(int id, CancellationToken ct = default);

        Task<List<TestResult>> GetEntitiesByRecordIdAsync(int recordId, CancellationToken ct = default);

        Task<TestResult> CreateAsync(TestResult entity, CancellationToken ct = default);
        Task UpdateAsync(TestResult entity, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);

        Task<bool> RecordExistsAsync(int recordId, CancellationToken ct = default);
        Task<bool> TestTypeExistsAsync(int testTypeId, CancellationToken ct = default);

        Task<List<MedicalRecord>> GetWorklistEntitiesAsync(
            DateOnly? visitDate,
            string? patientName,
            CancellationToken ct = default);

        Task<List<Service>> GetTestTypeEntitiesAsync(CancellationToken ct = default);

        Task EnsureMedicalServiceForTestAsync(int recordId, int serviceId, CancellationToken ct = default);
    }
}
