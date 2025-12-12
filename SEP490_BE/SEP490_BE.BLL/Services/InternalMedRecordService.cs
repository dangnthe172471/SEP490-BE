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
                throw new KeyNotFoundException($"Không tìm thấy hồ sơ khám bệnh với mã {dto.RecordId}.");

            if (await _internalRepo.HasInternalMedAsync(dto.RecordId, ct))
                throw new InvalidOperationException("Đã tồn tại hồ sơ khám nội cho hồ sơ khám bệnh này.");

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
                ?? throw new KeyNotFoundException(
                    $"Không tìm thấy hồ sơ khám nội cho hồ sơ khám bệnh có mã {recordId}."
                );

            if (!dto.BloodPressure.HasValue ||
                !dto.HeartRate.HasValue ||
                !dto.BloodSugar.HasValue ||
                string.IsNullOrWhiteSpace(dto.Notes))
            {
                throw new ArgumentException(
                    "Các trường huyết áp, nhịp tim, đường huyết và ghi chú khi cập nhật không được để trống."
                );
            }

            entity.BloodPressure = dto.BloodPressure.Value;
            entity.HeartRate = dto.HeartRate.Value;
            entity.BloodSugar = dto.BloodSugar.Value;
            entity.Notes = dto.Notes.Trim();

            await _internalRepo.UpdateAsync(entity, ct);
            return Map(entity);
        }

        public Task DeleteAsync(int recordId, CancellationToken ct = default)
            => _internalRepo.DeleteAsync(recordId, ct);

        private async Task AddMedicalServiceAsync(int recordId, string category, CancellationToken ct)
        {
            var service = await _medicalServiceRepo.GetServiceByCategoryAsync(category, ct)
                ?? throw new InvalidOperationException(
                    $"Chưa cấu hình dịch vụ cho chuyên khoa '{category}'."
                );

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

        public async Task<(bool HasPediatric, bool HasInternalMed, bool HasDermatology)> CheckSpecialtiesAsync(
            int recordId,
            CancellationToken ct = default)
        {
            var record = await _medicalRecordRepo.GetByIdAsync(recordId, ct);
            if (record == null)
                throw new KeyNotFoundException($"Không tìm thấy hồ sơ khám bệnh với mã {recordId}.");

            var hasPedia = await _pediatricRepo.HasPediatricAsync(recordId, ct);
            var hasInternal = await _internalRepo.HasInternalMedAsync(recordId, ct);
            var hasDerm = await _dermRepo.HasDermatologyAsync(recordId, ct);

            return (hasPedia, hasInternal, hasDerm);
        }
    }
}
