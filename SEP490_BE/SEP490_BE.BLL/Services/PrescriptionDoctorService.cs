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

        public async Task<PrescriptionSummaryDto> CreateAsync(
            int userIdFromToken,
            CreatePrescriptionRequest req,
            CancellationToken ct)
        {
            var doctor = await _repo.GetDoctorByUserIdAsync(userIdFromToken, ct)
                         ?? throw new InvalidOperationException("Bác sĩ không tồn tại.");

            var record = await _repo.GetRecordWithAppointmentAsync(req.RecordId, ct)
                         ?? throw new InvalidOperationException("Hồ sơ bệnh án không tồn tại.");

            if (record.Appointment.DoctorId != doctor.DoctorId)
                throw new UnauthorizedAccessException("Bạn không phụ trách hồ sơ này.");

            if (req.Items.Count == 0)
                throw new InvalidOperationException("Đơn thuốc phải có ít nhất 1 dòng.");

            var medicineIds = req.Items.Select(i => i.MedicineId).Distinct().ToArray();

            // Validate MedicineId
            var meds = await _repo.GetMedicinesByIdsAsync(medicineIds, ct);
            if (meds.Count != medicineIds.Length)
                throw new InvalidOperationException("Một hoặc nhiều thuốc không hợp lệ.");

            // Lấy snapshot version mới nhất
            var latestVersions = await _repo.GetLatestMedicineVersionsByMedicineIdsAsync(medicineIds, ct);

            var header = new Prescription
            {
                RecordId = record.RecordId,
                DoctorId = doctor.DoctorId,
                IssuedDate = req.IssuedDate ?? DateTime.UtcNow,
                // Notes = req.Notes
            };

            var details = req.Items.Select(i =>
            {
                var v = latestVersions[i.MedicineId];

                return new PrescriptionDetail
                {
                    MedicineId = i.MedicineId,
                    MedicineVersionId = v.MedicineVersionId,
                    Dosage = i.Dosage,
                    Duration = i.Duration,
                    Instruction = i.Instruction
                };
            }).ToList();

            var created = await _repo.CreatePrescriptionAsync(header, details, ct);
            var appt = record.Appointment;

            var lines = details.Select(d =>
            {
                var v = latestVersions[d.MedicineId];

                return new PrescriptionLineDto
                {
                    PrescriptionDetailId = d.PrescriptionDetailId,
                    MedicineId = v.MedicineId,
                    MedicineName = v.MedicineName,

                    ActiveIngredient = v.ActiveIngredient,
                    Strength = v.Strength,
                    DosageForm = v.DosageForm,
                    Route = v.Route,
                    PrescriptionUnit = v.PrescriptionUnit,
                    TherapeuticClass = v.TherapeuticClass,
                    PackSize = v.PackSize,
                    CommonSideEffects = v.CommonSideEffects,
                    NoteForDoctor = v.NoteForDoctor,

                    Dosage = d.Dosage,
                    Duration = d.Duration,
                    Instruction = d.Instruction,

                    ProviderId = v.ProviderId,
                    ProviderName = v.ProviderName,
                    ProviderContact = v.ProviderContact
                };
            }).ToList();

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
                Items = lines,
                Notes = req.Notes
            };
        }

        public async Task<PrescriptionSummaryDto?> GetByIdAsync(
            int userIdFromToken,
            int prescriptionId,
            CancellationToken ct)
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
                Items = pres.PrescriptionDetails.Select(d =>
                {
                    var v = d.MedicineVersion;

                    return new PrescriptionLineDto
                    {
                        PrescriptionDetailId = d.PrescriptionDetailId,
                        MedicineId = v.MedicineId,
                        MedicineName = v.MedicineName,

                        ActiveIngredient = v.ActiveIngredient,
                        Strength = v.Strength,
                        DosageForm = v.DosageForm,
                        Route = v.Route,
                        PrescriptionUnit = v.PrescriptionUnit,
                        TherapeuticClass = v.TherapeuticClass,
                        PackSize = v.PackSize,
                        CommonSideEffects = v.CommonSideEffects,
                        NoteForDoctor = v.NoteForDoctor,

                        Dosage = d.Dosage,
                        Duration = d.Duration,
                        Instruction = d.Instruction,

                        ProviderId = v.ProviderId,
                        ProviderName = v.ProviderName,
                        ProviderContact = v.ProviderContact
                    };
                }).ToList(),
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

