using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IMedicineRepository
    {
        Task<List<Medicine>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Medicine?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task CreateAsync(Medicine medicine, CancellationToken cancellationToken = default);
        Task UpdateAsync(Medicine medicine, CancellationToken cancellationToken = default);
        Task<List<Medicine>> GetByProviderIdAsync(int providerId, CancellationToken cancellationToken = default);
        Task SoftDeleteAsync(int medicineId, CancellationToken cancellationToken = default);

        Task<PharmacyProvider?> GetByUserIdAsync(int userId, CancellationToken ct = default);
        Task<int?> GetProviderIdByUserIdAsync(int userId, CancellationToken ct = default);

        Task<(List<Medicine> Items, int TotalCount)> GetByProviderIdPagedAsync(
        int providerId, int pageNumber, int pageSize, CancellationToken ct = default);
    }
}
