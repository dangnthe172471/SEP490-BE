using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.InternalMedRecordsDTO;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.BLL.Services
{
    public class InternalMedRecordService : IInternalMedRecordService
    {
        private readonly IInternalMedRecordRepository _repo;
        public InternalMedRecordService(IInternalMedRecordRepository repo) => _repo = repo;

        public async Task<ReadInternalMedRecordDto?> GetByRecordIdAsync(int recordId, CancellationToken ct = default)
        {
            var entity = await _repo.GetByRecordIdAsync(recordId, ct);
            return entity == null ? null : Map(entity);
        }

        public async Task<ReadInternalMedRecordDto> CreateAsync(CreateInternalMedRecordDto dto, CancellationToken ct = default)
        {
            if (!await _repo.MedicalRecordExistsAsync(dto.RecordId, ct))
                throw new KeyNotFoundException($"MedicalRecord {dto.RecordId} không tồn tại");

            // CHỈ chặn trùng InternalMed cho cùng RecordId
            if (await _repo.HasInternalMedAsync(dto.RecordId, ct))
                throw new InvalidOperationException("InternalMedRecord đã tồn tại cho MedicalRecord này.");

            var entity = new InternalMedRecord
            {
                RecordId = dto.RecordId,
                BloodPressure = dto.BloodPressure,
                HeartRate = dto.HeartRate,
                BloodSugar = dto.BloodSugar,
                Notes = dto.Notes?.Trim()
            };

            var created = await _repo.CreateAsync(entity, ct);
            return Map(created);
        }

        public async Task<ReadInternalMedRecordDto> UpdateAsync(int recordId, UpdateInternalMedRecordDto dto, CancellationToken ct = default)
        {
            var entity = await _repo.GetByRecordIdAsync(recordId, ct)
                ?? throw new KeyNotFoundException($"InternalMedRecord for RecordId {recordId} không tồn tại");

            if (dto.BloodPressure.HasValue) entity.BloodPressure = dto.BloodPressure;
            if (dto.HeartRate.HasValue) entity.HeartRate = dto.HeartRate;
            if (dto.BloodSugar.HasValue) entity.BloodSugar = dto.BloodSugar;
            if (dto.Notes != null) entity.Notes = dto.Notes.Trim();

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

        private static ReadInternalMedRecordDto Map(InternalMedRecord e) => new()
        {
            RecordId = e.RecordId,
            BloodPressure = e.BloodPressure,
            HeartRate = e.HeartRate,
            BloodSugar = e.BloodSugar,
            Notes = e.Notes
        };

    }
}
