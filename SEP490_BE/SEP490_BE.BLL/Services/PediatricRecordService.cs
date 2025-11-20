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
            var record = await _medicalRecordRepo.GetByIdAsync(dto.RecordId, ct);
            if (record == null)
                throw new KeyNotFoundException($"MedicalRecord {dto.RecordId} không tồn tại");

            if (await _pediatricRepo.HasPediatricAsync(dto.RecordId, ct))
                throw new InvalidOperationException("PediatricRecord đã tồn tại cho MedicalRecord này.");

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
            var entity = await _pediatricRepo.GetByRecordIdAsync(recordId, ct)
                ?? throw new KeyNotFoundException($"PediatricRecord cho RecordId {recordId} không tồn tại");

            if (dto.WeightKg.HasValue) entity.WeightKg = dto.WeightKg;
            if (dto.HeightCm.HasValue) entity.HeightCm = dto.HeightCm;
            if (dto.HeartRate.HasValue) entity.HeartRate = dto.HeartRate;
            if (dto.TemperatureC.HasValue) entity.TemperatureC = dto.TemperatureC;

            await _pediatricRepo.UpdateAsync(entity, ct);
            return Map(entity);
        }

        public Task DeleteAsync(int recordId, CancellationToken ct = default)
            => _pediatricRepo.DeleteAsync(recordId, ct);

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
