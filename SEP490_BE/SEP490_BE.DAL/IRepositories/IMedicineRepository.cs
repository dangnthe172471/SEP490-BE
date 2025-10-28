using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IMedicineRepository
    {
        Task<List<Medicine>> GetAllMedicineAsync(CancellationToken cancellationToken = default);
        Task<Medicine?> GetMedicineByIdAsync(int id, CancellationToken cancellationToken = default);
        Task CreateMedicineAsync(Medicine medicine, CancellationToken cancellationToken = default);
        Task UpdateMedicineAsync(Medicine medicine, CancellationToken cancellationToken = default);
        Task<List<Medicine>> GetByProviderIdAsync(int providerId, CancellationToken cancellationToken = default);
        Task SoftDeleteAsync(int medicineId, CancellationToken cancellationToken = default);

        Task<PharmacyProvider?> GetByUserIdAsync(int userId, CancellationToken ct = default);
        Task<int?> GetProviderIdByUserIdAsync(int userId, CancellationToken ct = default);

        Task<(List<Medicine> Items, int TotalCount)> GetByProviderIdPagedAsync(
            int providerId,
            int pageNumber,
            int pageSize,
            string? status = null,
            string? sort = null,
            CancellationToken ct = default);

    }
}
