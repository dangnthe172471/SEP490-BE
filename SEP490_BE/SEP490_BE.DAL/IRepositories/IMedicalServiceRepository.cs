using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IMedicalServiceRepository
    {
        Task<Service?> GetServiceByCategoryAsync(string category, CancellationToken ct = default);

        Task<bool> MedicalServiceExistsAsync(int recordId, int serviceId, CancellationToken ct = default);

        Task<MedicalService> CreateMedicalServiceAsync(
            int recordId,
            int serviceId,
            decimal unitPrice,
            string notes,
            CancellationToken ct = default);
    }
}
