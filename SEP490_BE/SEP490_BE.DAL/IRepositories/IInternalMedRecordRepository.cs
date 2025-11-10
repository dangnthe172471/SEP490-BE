using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IInternalMedRecordRepository
    {
        Task<InternalMedRecord?> GetByRecordIdAsync(int recordId, CancellationToken ct = default);
        Task<bool> MedicalRecordExistsAsync(int recordId, CancellationToken ct = default);
        Task<bool> HasInternalMedAsync(int recordId, CancellationToken ct = default);
        Task<bool> HasPediatricAsync(int recordId, CancellationToken ct = default);
        Task<InternalMedRecord> CreateAsync(InternalMedRecord entity, CancellationToken ct = default);
        Task UpdateAsync(InternalMedRecord entity, CancellationToken ct = default);
        Task DeleteAsync(int recordId, CancellationToken ct = default);
    }
}
