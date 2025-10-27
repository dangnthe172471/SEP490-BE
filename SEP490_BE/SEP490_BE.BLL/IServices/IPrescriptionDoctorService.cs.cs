using SEP490_BE.DAL.DTOs.PrescriptionDoctorDTO;

namespace SEP490_BE.BLL.IServices
{
    public interface IPrescriptionDoctorService
    {
        Task<PrescriptionSummaryDto> CreateAsync(int userIdFromToken, CreatePrescriptionRequest req, CancellationToken ct);
        Task<PrescriptionSummaryDto?> GetByIdAsync(int userIdFromToken, int prescriptionId, CancellationToken ct);
    }
}
