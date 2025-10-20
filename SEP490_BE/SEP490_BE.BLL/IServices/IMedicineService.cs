using SEP490_BE.DAL.DTOs.MedicineDTO;

namespace SEP490_BE.BLL.IServices
{
    public interface IMedicineService
    {
        Task<List<ReadMedicineDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ReadMedicineDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task CreateAsync(CreateMedicineDto dto, int providerId, CancellationToken cancellationToken = default);
        Task UpdateAsync(int id, UpdateMedicineDto dto, CancellationToken cancellationToken = default);
        Task<List<ReadMedicineDto>> GetByProviderIdAsync(int providerId, CancellationToken cancellationToken = default);
        Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

        Task<int?> GetProviderIdByUserIdAsync(int userId, CancellationToken ct = default);

        Task<List<ReadMedicineDto>> GetMineAsync(int userId, CancellationToken ct = default);
        Task<PagedResult<ReadMedicineDto>> GetMinePagedAsync(
        int userId, int pageNumber, int pageSize, CancellationToken ct = default);
    }
}
