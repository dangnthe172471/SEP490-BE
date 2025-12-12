using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.PrescriptionDoctorDTO;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IPrescriptionDoctorRepository
    {
        Task<Doctor?> GetDoctorByUserIdAsync(int userId, CancellationToken ct);

        Task<MedicalRecord?> GetRecordWithAppointmentAsync(int recordId, CancellationToken ct);

        Task<Dictionary<int, Medicine>> GetMedicinesByIdsAsync(
            IEnumerable<int> ids,
            CancellationToken ct);

        Task<Dictionary<int, MedicineVersion>> GetLatestMedicineVersionsByMedicineIdsAsync(
            IEnumerable<int> ids,
            CancellationToken ct);

        Task<Prescription> CreatePrescriptionAsync(
            Prescription header,
            IEnumerable<PrescriptionDetail> details,
            CancellationToken ct);

        Task<Prescription?> GetPrescriptionGraphAsync(
            int prescriptionId,
            CancellationToken ct);

        Task<(List<MedicalRecord> Items, int TotalCount)> GetRecordsForDoctorAsync(
            int doctorId,
            DateOnly? visitDateFrom,
            DateOnly? visitDateTo,
            string? patientNameSearch,
            int pageNumber,
            int pageSize,
            CancellationToken ct);

        Task<Dictionary<int, int?>> GetLatestPrescriptionIdsByRecordIdsAsync(
            IEnumerable<int> recordIds,
            CancellationToken ct);
    }
}
