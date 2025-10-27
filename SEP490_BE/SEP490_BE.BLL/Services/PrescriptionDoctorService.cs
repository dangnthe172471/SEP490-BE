using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.PrescriptionDoctorDTO;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.BLL.Services
{
    public class PrescriptionDoctorService : IPrescriptionDoctorService
    {
        private readonly IPrescriptionDoctorRepository _repo;
        public PrescriptionDoctorService(IPrescriptionDoctorRepository repo) => _repo = repo;

        public async Task<PrescriptionSummaryDto> CreateAsync(int userIdFromToken, CreatePrescriptionRequest req, CancellationToken ct)
        {
            var doctor = await _repo.GetDoctorByUserIdAsync(userIdFromToken, ct)
                         ?? throw new InvalidOperationException("Bác sĩ không tồn tại.");

            var record = await _repo.GetRecordWithAppointmentAsync(req.RecordId, ct)
                         ?? throw new InvalidOperationException("Hồ sơ bệnh án không tồn tại.");

            if (record.Appointment.DoctorId != doctor.DoctorId)
                throw new UnauthorizedAccessException("Bạn không phụ trách hồ sơ này.");

            if (req.Items.Count == 0)
                throw new InvalidOperationException("Đơn thuốc phải có ít nhất 1 dòng.");

            var meds = await _repo.GetMedicinesByIdsAsync(req.Items.Select(i => i.MedicineId), ct);
            if (meds.Count != req.Items.Select(i => i.MedicineId).Distinct().Count())
                throw new InvalidOperationException("Một hoặc nhiều thuốc không hợp lệ.");

            var header = new Prescription
            {
                RecordId = record.RecordId,
                DoctorId = doctor.DoctorId,
                IssuedDate = req.IssuedDate ?? DateTime.UtcNow
            };

            var details = req.Items.Select(i => new PrescriptionDetail
            {
                MedicineId = i.MedicineId,
                Dosage = i.Dosage,
                Duration = i.Duration
            }).ToList();

            var created = await _repo.CreatePrescriptionAsync(header, details, ct);

            var lines = details.Select(d =>
            {
                var med = meds[d.MedicineId];
                return new PrescriptionLineDto
                {
                    PrescriptionDetailId = d.PrescriptionDetailId,
                    MedicineId = med.MedicineId,
                    MedicineName = med.MedicineName,
                    Dosage = d.Dosage,
                    Duration = d.Duration,
                    ProviderId = med.ProviderId,
                    ProviderName = med.Provider.User?.FullName ?? "Nhà cung cấp không xác định",
                    ProviderContact = med.Provider.Contact
                };
            }).ToList();

            var appt = record.Appointment;
            return new PrescriptionSummaryDto
            {
                PrescriptionId = created.PrescriptionId,
                IssuedDate = created.IssuedDate,
                Diagnosis = BuildDiagnosis(record.Diagnosis),
                Doctor = new PrescriptionDoctorInfoDto
                {
                    DoctorId = appt.DoctorId,
                    Name = appt.Doctor.User?.FullName ?? $"BS#{appt.DoctorId}",
                    Specialty = appt.Doctor.Specialty,
                    Phone = appt.Doctor.User?.Phone
                },
                Patient = new PrescriptionPatientInfoDto
                {
                    PatientId = appt.PatientId,
                    Name = appt.Patient.User?.FullName ?? $"BN#{appt.PatientId}",
                    Gender = appt.Patient.User?.Gender,
                    Dob = appt.Patient.User?.Dob?.ToString("yyyy-MM-dd"),
                    Phone = appt.Patient.User?.Phone,
                },
                Items = lines
            };
        }

        public async Task<PrescriptionSummaryDto?> GetByIdAsync(int userIdFromToken, int prescriptionId, CancellationToken ct)
        {
            var pres = await _repo.GetPrescriptionGraphAsync(prescriptionId, ct);
            if (pres is null) return null;

            var appt = pres.Record.Appointment;

            return new PrescriptionSummaryDto
            {
                PrescriptionId = pres.PrescriptionId,
                IssuedDate = pres.IssuedDate,
                Diagnosis = BuildDiagnosis(pres.Record.Diagnosis),
                Doctor = new PrescriptionDoctorInfoDto
                {
                    DoctorId = appt.DoctorId,
                    Name = appt.Doctor.User?.FullName ?? $"BS#{appt.DoctorId}",
                    Specialty = appt.Doctor.Specialty,
                    Phone = appt.Doctor.User?.Phone
                },
                Patient = new PrescriptionPatientInfoDto
                {
                    PatientId = appt.PatientId,
                    Name = appt.Patient.User?.FullName ?? $"BN#{appt.PatientId}",
                    Gender = appt.Patient.User?.Gender,
                    Dob = appt.Patient.User?.Dob?.ToString("yyyy-MM-dd"),
                    Phone = appt.Patient.User?.Phone,
                },
                Items = pres.PrescriptionDetails.Select(d => new PrescriptionLineDto
                {
                    PrescriptionDetailId = d.PrescriptionDetailId,
                    MedicineId = d.MedicineId,
                    MedicineName = d.Medicine.MedicineName,
                    Dosage = d.Dosage,
                    Duration = d.Duration,
                    ProviderId = d.Medicine.ProviderId,
                    ProviderName = d.Medicine.Provider.User?.FullName ?? "Nhà cung cấp không xác định",
                    ProviderContact = d.Medicine.Provider.Contact
                }).ToList()
            };
        }

        private static DiagnosisInfoDto BuildDiagnosis(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return new();
            var parts = raw.Split('-', 2, StringSplitOptions.TrimEntries);
            return parts.Length == 2
                ? new DiagnosisInfoDto { Code = parts[0], Text = parts[1] }
                : new DiagnosisInfoDto { Text = raw };
        }

        public Task<PagedResult<RecordListItemDto>> GetRecordsForDoctorAsync(
            int userIdFromToken,
            DateOnly? visitDateFrom,
            DateOnly? visitDateTo,
            string? patientNameSearch,
            int pageNumber,
            int pageSize,
            CancellationToken ct)
            => _repo.GetRecordsForDoctorAsync(userIdFromToken, visitDateFrom, visitDateTo, patientNameSearch, pageNumber, pageSize, ct);
    }
}
