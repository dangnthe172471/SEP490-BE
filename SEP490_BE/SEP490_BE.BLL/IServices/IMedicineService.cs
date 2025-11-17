using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.MedicineDTO;

namespace SEP490_BE.BLL.IServices
{
    public interface IMedicineService
    {
        Task<List<ReadMedicineDto>> GetAllMedicineAsync(CancellationToken cancellationToken = default);
        Task<ReadMedicineDto?> GetMedicineByIdAsync(int id, CancellationToken cancellationToken = default);
        Task CreateMedicineAsync(CreateMedicineDto dto, int providerId, CancellationToken cancellationToken = default);

        Task UpdateMineAsync(int userId, int id, UpdateMedicineDto dto, CancellationToken cancellationToken = default);

        Task<int?> GetProviderIdByUserIdAsync(int userId, CancellationToken ct = default);

        Task<PagedResult<ReadMedicineDto>> GetMinePagedAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? status = null,
            string? sort = null,
            CancellationToken ct = default);

        Task<byte[]> GenerateExcelTemplateAsync(CancellationToken ct = default);

        Task<BulkImportResultDto> ImportFromExcelAsync(
            int userId,
            Stream excelStream,
            CancellationToken ct = default);
    }
}
