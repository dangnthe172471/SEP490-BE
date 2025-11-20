using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.DTOs.DermatologyDTO;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.BLL.Services
{
    public class DermatologyRecordService : IDermatologyRecordService
    {
        private readonly IDermatologyRecordRepository _dermRepo;
        private readonly IMedicalRecordRepository _medicalRecordRepo;
        private readonly IMedicalServiceRepository _medicalServiceRepo;

        public DermatologyRecordService(
            IDermatologyRecordRepository dermRepo,
            IMedicalRecordRepository medicalRecordRepo,
            IMedicalServiceRepository medicalServiceRepo)
        {
            _dermRepo = dermRepo;
            _medicalRecordRepo = medicalRecordRepo;
            _medicalServiceRepo = medicalServiceRepo;
        }

        public async Task<ReadDermatologyRecordDto?> GetByRecordIdAsync(int recordId, CancellationToken ct = default)
        {
            var entity = await _dermRepo.GetByRecordIdAsync(recordId, ct);
            return entity == null ? null : Map(entity);
        }

        public async Task<ReadDermatologyRecordDto> CreateAsync(CreateDermatologyRecordDto dto, CancellationToken ct = default)
        {
            var record = await _medicalRecordRepo.GetByIdAsync(dto.RecordId, ct);
            if (record == null)
                throw new KeyNotFoundException($"MedicalRecord {dto.RecordId} không tồn tại");

            if (await _dermRepo.HasDermatologyAsync(dto.RecordId, ct))
                throw new InvalidOperationException("DermatologyRecord đã tồn tại cho MedicalRecord này.");

            var created = await _dermRepo.CreateAsync(new DermatologyRecord
            {
                RecordId = dto.RecordId,
                RequestedProcedure = dto.RequestedProcedure?.Trim() ?? "Khám da liễu",
                BodyArea = dto.BodyArea,
                ProcedureNotes = dto.ProcedureNotes,
                PerformedAt = DateTime.UtcNow
            }, ct);

            await AddMedicalServiceAsync(dto.RecordId, ServiceCategories.Dermatology, ct);

            return Map(created);
        }

        public async Task<ReadDermatologyRecordDto> UpdateAsync(
            int recordId,
            UpdateDermatologyRecordDto dto,
            CancellationToken ct = default)
        {
            var entity = await _dermRepo.GetByRecordIdAsync(recordId, ct)
                ?? throw new KeyNotFoundException($"DermatologyRecord cho RecordId {recordId} không tồn tại");

            if (dto.RequestedProcedure != null)
                entity.RequestedProcedure = dto.RequestedProcedure.Trim();
            if (dto.BodyArea != null)
                entity.BodyArea = dto.BodyArea;
            if (dto.ProcedureNotes != null)
                entity.ProcedureNotes = dto.ProcedureNotes;
            if (dto.ResultSummary != null)
                entity.ResultSummary = dto.ResultSummary;
            if (dto.Attachment != null)
                entity.Attachment = dto.Attachment;
            if (dto.PerformedByUserId.HasValue)
            {
                entity.PerformedByUserId = dto.PerformedByUserId.Value;
                entity.PerformedAt = DateTime.UtcNow;
            }
            await _dermRepo.UpdateAsync(entity, ct);
            return Map(entity);
        }

        public Task DeleteAsync(int recordId, CancellationToken ct = default)
            => _dermRepo.DeleteAsync(recordId, ct);

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

        private static ReadDermatologyRecordDto Map(DermatologyRecord e) => new()
        {
            DermRecordId = e.DermRecordId,
            RecordId = e.RecordId,
            RequestedProcedure = e.RequestedProcedure,
            BodyArea = e.BodyArea,
            ProcedureNotes = e.ProcedureNotes,
            ResultSummary = e.ResultSummary,
            Attachment = e.Attachment,
            PerformedAt = e.PerformedAt,
            PerformedByUserId = e.PerformedByUserId
        };
    }
}
