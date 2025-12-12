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
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Dữ liệu tạo hồ sơ da liễu không được để trống.");
            }

            ValidateCreate(dto);

            var record = await _medicalRecordRepo.GetByIdAsync(dto.RecordId, ct);
            if (record == null)
                throw new KeyNotFoundException($"Phiếu khám (MedicalRecord) với mã {dto.RecordId} không tồn tại.");

            if (await _dermRepo.HasDermatologyAsync(dto.RecordId, ct))
                throw new InvalidOperationException("Hồ sơ da liễu đã tồn tại cho phiếu khám này.");

            var created = await _dermRepo.CreateAsync(new DermatologyRecord
            {
                RecordId = dto.RecordId,
                RequestedProcedure = string.IsNullOrWhiteSpace(dto.RequestedProcedure)
                    ? "Khám da liễu"
                    : dto.RequestedProcedure.Trim(),
                BodyArea = dto.BodyArea?.Trim(),
                ProcedureNotes = dto.ProcedureNotes?.Trim(),
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
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Dữ liệu cập nhật hồ sơ da liễu không được để trống.");
            }

            var hasAnyField =
                dto.RequestedProcedure != null ||
                dto.BodyArea != null ||
                dto.ProcedureNotes != null ||
                dto.ResultSummary != null ||
                dto.Attachment != null ||
                dto.PerformedByUserId.HasValue;

            if (!hasAnyField)
            {
                throw new ArgumentException("Không có dữ liệu nào để cập nhật hồ sơ da liễu.", nameof(dto));
            }

            ValidateUpdate(dto);

            var entity = await _dermRepo.GetByRecordIdAsync(recordId, ct)
                ?? throw new KeyNotFoundException($"Không tìm thấy hồ sơ da liễu cho phiếu khám có mã {recordId}.");

            if (dto.RequestedProcedure != null)
                entity.RequestedProcedure = string.IsNullOrWhiteSpace(dto.RequestedProcedure)
                    ? entity.RequestedProcedure
                    : dto.RequestedProcedure.Trim();

            if (dto.BodyArea != null)
                entity.BodyArea = string.IsNullOrWhiteSpace(dto.BodyArea)
                    ? entity.BodyArea
                    : dto.BodyArea.Trim();

            if (dto.ProcedureNotes != null)
                entity.ProcedureNotes = string.IsNullOrWhiteSpace(dto.ProcedureNotes)
                    ? null
                    : dto.ProcedureNotes.Trim();

            if (dto.ResultSummary != null)
                entity.ResultSummary = string.IsNullOrWhiteSpace(dto.ResultSummary)
                    ? null
                    : dto.ResultSummary.Trim();

            if (dto.Attachment != null)
                entity.Attachment = string.IsNullOrWhiteSpace(dto.Attachment)
                    ? null
                    : dto.Attachment.Trim();

            if (dto.PerformedByUserId.HasValue)
            {
                entity.PerformedByUserId = dto.PerformedByUserId.Value;
                entity.PerformedAt = DateTime.UtcNow;
            }

            await _dermRepo.UpdateAsync(entity, ct);
            return Map(entity);
        }

        public async Task DeleteAsync(int recordId, CancellationToken ct = default)
        {
            var entity = await _dermRepo.GetByRecordIdAsync(recordId, ct);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy hồ sơ da liễu cho phiếu khám có mã {recordId}.");
            }

            await _dermRepo.DeleteAsync(recordId, ct);
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

        #region Private validation helpers

        private void ValidateCreate(CreateDermatologyRecordDto dto)
        {
            var errors = new List<string>();

            if (dto.RecordId <= 0)
                errors.Add("Mã phiếu khám không hợp lệ.");

            if (!string.IsNullOrWhiteSpace(dto.RequestedProcedure) &&
                dto.RequestedProcedure.Trim().Length > 200)
            {
                errors.Add("Tên thủ thuật/yêu cầu da liễu không được dài quá 200 ký tự.");
            }

            if (string.IsNullOrWhiteSpace(dto.BodyArea))
            {
                errors.Add("Vùng da thực hiện thủ thuật không được để trống.");
            }

            if (!string.IsNullOrWhiteSpace(dto.BodyArea) &&
                dto.BodyArea.Trim().Length > 200)
            {
                errors.Add("Tên vùng da không được dài quá 200 ký tự.");
            }

            if (errors.Count > 0)
            {
                throw new ArgumentException(string.Join(" ", errors));
            }
        }

        private void ValidateUpdate(UpdateDermatologyRecordDto dto)
        {
            var errors = new List<string>();

            if (dto.RequestedProcedure != null &&
                string.IsNullOrWhiteSpace(dto.RequestedProcedure))
            {
                errors.Add("Tên thủ thuật/yêu cầu da liễu không được để trống nếu cập nhật.");
            }

            if (dto.BodyArea != null &&
                string.IsNullOrWhiteSpace(dto.BodyArea))
            {
                errors.Add("Vùng da thực hiện thủ thuật không được để trống nếu cập nhật.");
            }

            if (dto.RequestedProcedure != null &&
                !string.IsNullOrWhiteSpace(dto.RequestedProcedure) &&
                dto.RequestedProcedure.Trim().Length > 200)
            {
                errors.Add("Tên thủ thuật/yêu cầu da liễu không được dài quá 200 ký tự.");
            }

            if (dto.BodyArea != null &&
                !string.IsNullOrWhiteSpace(dto.BodyArea) &&
                dto.BodyArea.Trim().Length > 200)
            {
                errors.Add("Tên vùng da không được dài quá 200 ký tự.");
            }

            if (errors.Count > 0)
            {
                throw new ArgumentException(string.Join(" ", errors));
            }
        }

        #endregion
    }
}
