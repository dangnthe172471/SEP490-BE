using SEP490_BE.DAL.DTOs.MedicineDTO;

namespace SEP490_BE.BLL.IServices
{
    public interface IMedicineService
    {
        Task<List<ReadMedicineDto>> GetAllMedicineAsync(CancellationToken cancellationToken = default);
        Task<ReadMedicineDto?> GetMedicineByIdAsync(int id, CancellationToken cancellationToken = default);
        Task CreateMedicineAsync(CreateMedicineDto dto, int providerId, CancellationToken cancellationToken = default);
        Task UpdateMedicineAsync(int id, UpdateMedicineDto dto, CancellationToken cancellationToken = default);
        Task<List<ReadMedicineDto>> GetByProviderIdAsync(int providerId, CancellationToken cancellationToken = default);
        Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

        Task<int?> GetProviderIdByUserIdAsync(int userId, CancellationToken ct = default);

        Task<List<ReadMedicineDto>> GetMineAsync(int userId, CancellationToken ct = default);

        Task<PagedResult<ReadMedicineDto>> GetMinePagedAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? status = null,
            string? sort = null,
            CancellationToken ct = default);

    }
}
