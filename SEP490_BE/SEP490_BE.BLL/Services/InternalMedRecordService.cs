using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.DTOs.InternalMedRecordsDTO;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.BLL.Services
{
    public class InternalMedRecordService : IInternalMedRecordService
    {
        private readonly IInternalMedRecordRepository _internalRepo;
        private readonly IPediatricRecordRepository _pediatricRepo;
        private readonly IDermatologyRecordRepository _dermRepo;
        private readonly IMedicalRecordRepository _medicalRecordRepo;
        private readonly IMedicalServiceRepository _medicalServiceRepo;

        public InternalMedRecordService(
            IInternalMedRecordRepository internalRepo,
            IPediatricRecordRepository pediatricRepo,
            IDermatologyRecordRepository dermRepo,
            IMedicalRecordRepository medicalRecordRepo,
            IMedicalServiceRepository medicalServiceRepo)
        {
            _internalRepo = internalRepo;
            _pediatricRepo = pediatricRepo;
            _dermRepo = dermRepo;  
            _medicalRecordRepo = medicalRecordRepo;
            _medicalServiceRepo = medicalServiceRepo;
        }

        public async Task<ReadInternalMedRecordDto?> GetByRecordIdAsync(int recordId, CancellationToken ct = default)
        {
            var entity = await _internalRepo.GetByRecordIdAsync(recordId, ct);
            return entity == null ? null : Map(entity);
        }

        public async Task<ReadInternalMedRecordDto> CreateAsync(CreateInternalMedRecordDto dto, CancellationToken ct = default)
        {
            var record = await _medicalRecordRepo.GetByIdAsync(dto.RecordId, ct);
            if (record == null)
                throw new KeyNotFoundException($"MedicalRecord {dto.RecordId} không tồn tại");

            if (await _internalRepo.HasInternalMedAsync(dto.RecordId, ct))
                throw new InvalidOperationException("InternalMedRecord đã tồn tại cho MedicalRecord này.");

            var created = await _internalRepo.CreateAsync(new InternalMedRecord
            {
                RecordId = dto.RecordId,
                BloodPressure = dto.BloodPressure,
                HeartRate = dto.HeartRate,
                BloodSugar = dto.BloodSugar,
                Notes = dto.Notes?.Trim()
            }, ct);

            await AddMedicalServiceAsync(dto.RecordId, ServiceCategories.InternalMed, ct);

            return Map(created);
        }

        public async Task<ReadInternalMedRecordDto> UpdateAsync(int recordId, UpdateInternalMedRecordDto dto, CancellationToken ct = default)
        {
            var entity = await _internalRepo.GetByRecordIdAsync(recordId, ct)
                ?? throw new KeyNotFoundException($"InternalMedRecord cho RecordId {recordId} không tồn tại");

            if (dto.BloodPressure.HasValue) entity.BloodPressure = dto.BloodPressure;
            if (dto.HeartRate.HasValue) entity.HeartRate = dto.HeartRate;
            if (dto.BloodSugar.HasValue) entity.BloodSugar = dto.BloodSugar;
            if (dto.Notes != null) entity.Notes = dto.Notes.Trim();

            await _internalRepo.UpdateAsync(entity, ct);
            return Map(entity);
        }

        public Task DeleteAsync(int recordId, CancellationToken ct = default)
            => _internalRepo.DeleteAsync(recordId, ct);

        private async Task AddMedicalServiceAsync(int recordId, string category, CancellationToken ct)
        {
            var service = await _medicalServiceRepo.GetServiceByCategoryAsync(category, ct)
                ?? throw new InvalidOperationException($"Chưa cấu hình Service cho Category '{category}'");

            bool exists = await _medicalServiceRepo.MedicalServiceExistsAsync(recordId, service.ServiceId, ct);
            if (exists) return;

            await _medicalServiceRepo.CreateMedicalServiceAsync(
                recordId,
                service.ServiceId,
                service.Price ?? 0m,
                $"Khám {category}",
                ct
            );
        }

        private static ReadInternalMedRecordDto Map(InternalMedRecord e) => new()
        {
            RecordId = e.RecordId,
            BloodPressure = e.BloodPressure,
            HeartRate = e.HeartRate,
            BloodSugar = e.BloodSugar,
            Notes = e.Notes
        };

        public async Task<(bool HasPediatric, bool HasInternalMed, bool HasDermatology)> CheckSpecialtiesAsync(int recordId, CancellationToken ct = default)
        {
            var record = await _medicalRecordRepo.GetByIdAsync(recordId, ct);
            if (record == null)
                throw new KeyNotFoundException($"MedicalRecord {recordId} không tồn tại");

            var hasPedia = await _pediatricRepo.HasPediatricAsync(recordId, ct);
            var hasInternal = await _internalRepo.HasInternalMedAsync(recordId, ct);
            var hasDerm = await _dermRepo.HasDermatologyAsync(recordId, ct);

            return (hasPedia, hasInternal, hasDerm);
        }
    }
}
