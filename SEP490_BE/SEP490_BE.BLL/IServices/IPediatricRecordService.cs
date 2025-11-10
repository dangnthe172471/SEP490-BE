using SEP490_BE.DAL.DTOs.PediatricRecordsDTO;

namespace SEP490_BE.BLL.IServices
{
    public interface IPediatricRecordService
    {
        Task<ReadPediatricRecordDto?> GetByRecordIdAsync(int recordId, CancellationToken ct = default);
        Task<ReadPediatricRecordDto> CreateAsync(CreatePediatricRecordDto dto, CancellationToken ct = default);
        Task<ReadPediatricRecordDto> UpdateAsync(int recordId, UpdatePediatricRecordDto dto, CancellationToken ct = default);
        Task DeleteAsync(int recordId, CancellationToken ct = default);
    }
}
