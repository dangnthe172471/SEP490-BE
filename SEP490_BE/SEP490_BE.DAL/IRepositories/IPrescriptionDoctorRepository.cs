using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IPrescriptionDoctorRepository
    {
        Task<Doctor?> GetDoctorByUserIdAsync(int userId, CancellationToken ct);
        Task<MedicalRecord?> GetRecordWithAppointmentAsync(int recordId, CancellationToken ct);
        Task<Dictionary<int, Medicine>> GetMedicinesByIdsAsync(IEnumerable<int> ids, CancellationToken ct);
        Task<Prescription> CreatePrescriptionAsync(Prescription header, IEnumerable<PrescriptionDetail> details, CancellationToken ct);
        Task<Prescription?> GetPrescriptionGraphAsync(int prescriptionId, CancellationToken ct);
    }
}
