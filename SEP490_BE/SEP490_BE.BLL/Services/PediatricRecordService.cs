using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.DTOs.PediatricRecordsDTO;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.BLL.Services
{
    public class PediatricRecordService : IPediatricRecordService
    {
        private readonly IPediatricRecordRepository _pediatricRepo;
        private readonly IMedicalRecordRepository _medicalRecordRepo;
        private readonly IMedicalServiceRepository _medicalServiceRepo;

        public PediatricRecordService(
            IPediatricRecordRepository pediatricRepo,
            IMedicalRecordRepository medicalRecordRepo,
            IMedicalServiceRepository medicalServiceRepo)
        {
            _pediatricRepo = pediatricRepo;
            _medicalRecordRepo = medicalRecordRepo;
            _medicalServiceRepo = medicalServiceRepo;
        }

        public async Task<ReadPediatricRecordDto?> GetByRecordIdAsync(int recordId, CancellationToken ct = default)
        {
            var entity = await _pediatricRepo.GetByRecordIdAsync(recordId, ct);
            return entity == null ? null : Map(entity);
        }

        public async Task<ReadPediatricRecordDto> CreateAsync(CreatePediatricRecordDto dto, CancellationToken ct = default)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Dữ liệu tạo hồ sơ khám nhi không được để trống.");
            }

            var record = await _medicalRecordRepo.GetByIdAsync(dto.RecordId, ct);
            if (record == null)
                throw new KeyNotFoundException($"Phiếu khám (MedicalRecord) với mã {dto.RecordId} không tồn tại.");

            if (await _pediatricRepo.HasPediatricAsync(dto.RecordId, ct))
                throw new InvalidOperationException("Hồ sơ khám nhi đã tồn tại cho phiếu khám này.");

            var created = await _pediatricRepo.CreateAsync(new PediatricRecord
            {
                RecordId = dto.RecordId,
                WeightKg = dto.WeightKg,
                HeightCm = dto.HeightCm,
                HeartRate = dto.HeartRate,
                TemperatureC = dto.TemperatureC
            }, ct);

            await AddMedicalServiceAsync(dto.RecordId, ServiceCategories.Pediatric, ct);

            return Map(created);
        }

        public async Task<ReadPediatricRecordDto> UpdateAsync(int recordId, UpdatePediatricRecordDto dto, CancellationToken ct = default)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Dữ liệu cập nhật hồ sơ khám nhi không được để trống.");
            }

            var hasAnyField =
                dto.WeightKg.HasValue ||
                dto.HeightCm.HasValue ||
                dto.HeartRate.HasValue ||
                dto.TemperatureC.HasValue;

            if (!hasAnyField)
            {
                throw new ArgumentException("Không có dữ liệu nào để cập nhật hồ sơ khám nhi.", nameof(dto));
            }

            var entity = await _pediatricRepo.GetByRecordIdAsync(recordId, ct)
                ?? throw new KeyNotFoundException($"Không tìm thấy hồ sơ khám nhi cho phiếu khám có mã {recordId}.");

            if (dto.WeightKg.HasValue) entity.WeightKg = dto.WeightKg.Value;
            if (dto.HeightCm.HasValue) entity.HeightCm = dto.HeightCm.Value;
            if (dto.HeartRate.HasValue) entity.HeartRate = dto.HeartRate.Value;
            if (dto.TemperatureC.HasValue) entity.TemperatureC = dto.TemperatureC.Value;

            await _pediatricRepo.UpdateAsync(entity, ct);
            return Map(entity);
        }

        public async Task DeleteAsync(int recordId, CancellationToken ct = default)
        {
            var entity = await _pediatricRepo.GetByRecordIdAsync(recordId, ct);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy hồ sơ khám nhi cho phiếu khám có mã {recordId}.");
            }

            await _pediatricRepo.DeleteAsync(recordId, ct);
        }

        private async Task AddMedicalServiceAsync(int recordId, string category, CancellationToken ct)
        {
            var service = await _medicalServiceRepo.GetServiceByCategoryAsync(category, ct)
                ?? throw new InvalidOperationException($"Chưa cấu hình dịch vụ cho nhóm '{category}'.");

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

        private static ReadPediatricRecordDto Map(PediatricRecord e) => new()
        {
            RecordId = e.RecordId,
            WeightKg = e.WeightKg,
            HeightCm = e.HeightCm,
            HeartRate = e.HeartRate,
            TemperatureC = e.TemperatureC
        };
    }
}
