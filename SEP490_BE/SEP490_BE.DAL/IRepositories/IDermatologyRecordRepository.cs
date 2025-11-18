using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IDermatologyRecordRepository
    {
        Task<DermatologyRecord?> GetByRecordIdAsync(int recordId, CancellationToken ct = default);

        Task<bool> HasDermatologyAsync(int recordId, CancellationToken ct = default);

        Task<DermatologyRecord> CreateAsync(DermatologyRecord entity, CancellationToken ct = default);

        Task UpdateAsync(DermatologyRecord entity, CancellationToken ct = default);

        Task DeleteAsync(int recordId, CancellationToken ct = default);
    }
}
