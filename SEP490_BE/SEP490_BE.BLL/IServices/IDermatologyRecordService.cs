using SEP490_BE.DAL.DTOs.DermatologyDTO;

namespace SEP490_BE.BLL.IServices
{
    public interface IDermatologyRecordService
    {
        Task<ReadDermatologyRecordDto?> GetByRecordIdAsync(int recordId, CancellationToken ct = default);
        Task<ReadDermatologyRecordDto> CreateAsync(CreateDermatologyRecordDto dto, CancellationToken ct = default);
        Task<ReadDermatologyRecordDto> UpdateAsync(int recordId, UpdateDermatologyRecordDto dto, CancellationToken ct = default);
        Task DeleteAsync(int recordId, CancellationToken ct = default);
    }
}
