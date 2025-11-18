using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IPediatricRecordRepository
    {
        Task<PediatricRecord?> GetByRecordIdAsync(int recordId, CancellationToken ct = default);

        Task<bool> HasPediatricAsync(int recordId, CancellationToken ct = default);

        Task<PediatricRecord> CreateAsync(PediatricRecord entity, CancellationToken ct = default);

        Task UpdateAsync(PediatricRecord entity, CancellationToken ct = default);

        Task DeleteAsync(int recordId, CancellationToken ct = default);
    }
}
