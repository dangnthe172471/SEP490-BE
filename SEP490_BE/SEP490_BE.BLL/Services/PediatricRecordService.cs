using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.PediatricRecordsDTO;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.BLL.Services
{
    public class PediatricRecordService : IPediatricRecordService
    {
        private readonly IPediatricRecordRepository _repo;
        public PediatricRecordService(IPediatricRecordRepository repo) => _repo = repo;

        public async Task<ReadPediatricRecordDto?> GetByRecordIdAsync(int recordId, CancellationToken ct = default)
        {
            var entity = await _repo.GetByRecordIdAsync(recordId, ct);
            return entity == null ? null : Map(entity);
        }

        public async Task<ReadPediatricRecordDto> CreateAsync(CreatePediatricRecordDto dto, CancellationToken ct = default)
        {
            if (!await _repo.MedicalRecordExistsAsync(dto.RecordId, ct))
                throw new KeyNotFoundException($"MedicalRecord {dto.RecordId} không tồn tại");

            // CHỈ chặn trùng Pediatric cho cùng RecordId
            if (await _repo.HasPediatricAsync(dto.RecordId, ct))
                throw new InvalidOperationException("PediatricRecord đã tồn tại cho MedicalRecord này.");

            var entity = new PediatricRecord
            {
                RecordId = dto.RecordId,
                WeightKg = dto.WeightKg,
                HeightCm = dto.HeightCm,
                HeartRate = dto.HeartRate,
                TemperatureC = dto.TemperatureC
            };

            var created = await _repo.CreateAsync(entity, ct);
            return Map(created);
        }

        public async Task<ReadPediatricRecordDto> UpdateAsync(int recordId, UpdatePediatricRecordDto dto, CancellationToken ct = default)
        {
            var entity = await _repo.GetByRecordIdAsync(recordId, ct)
                ?? throw new KeyNotFoundException($"PediatricRecord for RecordId {recordId} không tồn tại");

            if (dto.WeightKg.HasValue) entity.WeightKg = dto.WeightKg;
            if (dto.HeightCm.HasValue) entity.HeightCm = dto.HeightCm;
            if (dto.HeartRate.HasValue) entity.HeartRate = dto.HeartRate;
            if (dto.TemperatureC.HasValue) entity.TemperatureC = dto.TemperatureC;

            await _repo.UpdateAsync(entity, ct);
            return Map(entity);
        }

        public Task DeleteAsync(int recordId, CancellationToken ct = default)
            => _repo.DeleteAsync(recordId, ct);

        // NEW: trả về trạng thái hiện có của 2 chuyên khoa
        public async Task<(bool HasPediatric, bool HasInternalMed)> CheckSpecialtiesAsync(int recordId, CancellationToken ct = default)
        {
            if (!await _repo.MedicalRecordExistsAsync(recordId, ct))
                throw new KeyNotFoundException($"MedicalRecord {recordId} không tồn tại");

            var hasPedia = await _repo.HasPediatricAsync(recordId, ct);
            var hasInternal = await _repo.HasInternalMedAsync(recordId, ct);
            return (hasPedia, hasInternal);
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
