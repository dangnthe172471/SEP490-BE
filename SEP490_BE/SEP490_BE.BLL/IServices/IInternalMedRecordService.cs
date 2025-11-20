using SEP490_BE.DAL.DTOs.InternalMedRecordsDTO;

namespace SEP490_BE.BLL.IServices
{
    public interface IInternalMedRecordService
    {
        Task<ReadInternalMedRecordDto?> GetByRecordIdAsync(int recordId, CancellationToken ct = default);
        Task<ReadInternalMedRecordDto> CreateAsync(CreateInternalMedRecordDto dto, CancellationToken ct = default);
        Task<ReadInternalMedRecordDto> UpdateAsync(int recordId, UpdateInternalMedRecordDto dto, CancellationToken ct = default);
        Task DeleteAsync(int recordId, CancellationToken ct = default);
        Task<(bool HasPediatric, bool HasInternalMed, bool HasDermatology)> CheckSpecialtiesAsync(int recordId, CancellationToken ct = default);
    }
}
