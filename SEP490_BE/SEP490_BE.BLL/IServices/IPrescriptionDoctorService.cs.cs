using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.PrescriptionDoctorDTO;

namespace SEP490_BE.BLL.IServices
{
    public interface IPrescriptionDoctorService
    {
        Task<PrescriptionSummaryDto> CreateAsync(int userIdFromToken, CreatePrescriptionRequest req, CancellationToken ct);
        Task<PrescriptionSummaryDto?> GetByIdAsync(int userIdFromToken, int prescriptionId, CancellationToken ct);
        Task<PagedResult<RecordListItemDto>> GetRecordsForDoctorAsync(
            int userIdFromToken,
            DateOnly? visitDateFrom,
            DateOnly? visitDateTo,
            string? patientNameSearch,
            int pageNumber,
            int pageSize,
            CancellationToken ct);
    }
}
