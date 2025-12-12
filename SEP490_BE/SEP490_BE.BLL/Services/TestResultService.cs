using Microsoft.Extensions.Options;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.TestReDTO;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.BLL.Services
{
    public class TestResultService : ITestResultService
    {
        private readonly ITestResultRepository _repo;

        public TestResultService(ITestResultRepository repo)
        {
            _repo = repo;
        }

        private static ReadTestResultDto MapToDto(TestResult tr)
            => new ReadTestResultDto
            {
                TestResultId = tr.TestResultId,
                RecordId = tr.RecordId,
                TestTypeId = tr.ServiceId,
                TestName = tr.Service?.ServiceName,
                ResultValue = tr.ResultValue,
                Unit = tr.Unit,
                Attachment = tr.Attachment,
                ResultDate = tr.ResultDate,
                Notes = tr.Notes
            };

        public async Task<ReadTestResultDto> CreateAsync(CreateTestResultDto dto, CancellationToken ct = default)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Dữ liệu tạo kết quả xét nghiệm không được để trống.");
            }

            ValidateCreate(dto);

            if (!await _repo.RecordExistsAsync(dto.RecordId, ct))
                throw new KeyNotFoundException($"Phiếu khám (MedicalRecord) với mã {dto.RecordId} không tồn tại.");

            if (!await _repo.TestTypeExistsAsync(dto.TestTypeId, ct))
                throw new KeyNotFoundException($"Loại xét nghiệm (TestType) với mã {dto.TestTypeId} không hợp lệ.");

            var entity = new TestResult
            {
                RecordId = dto.RecordId,
                ServiceId = dto.TestTypeId,
                ResultValue = dto.ResultValue.Trim(),
                Unit = dto.Unit?.Trim(),
                Attachment = dto.Attachment?.Trim(),
                ResultDate = dto.ResultDate ?? DateTime.UtcNow,
                Notes = dto.Notes?.Trim()
            };

            var created = await _repo.CreateAsync(entity, ct);
            await _repo.EnsureMedicalServiceForTestAsync(dto.RecordId, dto.TestTypeId, ct);

            var finalEntity = await _repo.GetEntityByIdAsync(created.TestResultId, ct)
                ?? throw new Exception("Không đọc được kết quả xét nghiệm sau khi tạo.");

            return MapToDto(finalEntity);
        }

        public async Task<ReadTestResultDto> UpdateAsync(int testResultId, UpdateTestResultDto dto, CancellationToken ct = default)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Dữ liệu cập nhật kết quả xét nghiệm không được để trống.");
            }

            var hasAnyField =
                dto.ResultValue != null ||
                dto.Unit != null ||
                dto.Attachment != null ||
                dto.ResultDate.HasValue ||
                dto.Notes != null;

            if (!hasAnyField)
            {
                throw new ArgumentException("Không có dữ liệu nào để cập nhật kết quả xét nghiệm.", nameof(dto));
            }

            ValidateUpdate(dto);

            var entity = await _repo.GetEntityByIdAsync(testResultId, ct)
                ?? throw new KeyNotFoundException($"Không tìm thấy kết quả xét nghiệm với mã {testResultId}.");

            if (dto.ResultValue != null)
            {
                var trimmed = dto.ResultValue.Trim();
                entity.ResultValue = trimmed;
            }

            if (dto.Unit != null)
            {
                var trimmed = dto.Unit.Trim();
                entity.Unit = string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
            }

            if (dto.Attachment != null)
            {
                var trimmed = dto.Attachment.Trim();
                entity.Attachment = string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
            }

            if (dto.ResultDate.HasValue)
            {
                entity.ResultDate = dto.ResultDate.Value;
            }

            if (dto.Notes != null)
            {
                var trimmed = dto.Notes.Trim();
                entity.Notes = string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
            }

            await _repo.UpdateAsync(entity, ct);

            var updated = await _repo.GetEntityByIdAsync(entity.TestResultId, ct)
                ?? throw new Exception("Không đọc được kết quả xét nghiệm sau khi cập nhật.");

            return MapToDto(updated);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _repo.GetEntityByIdAsync(id, ct);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy kết quả xét nghiệm với mã {id}.");
            }

            await _repo.DeleteAsync(id, ct);
        }

        public async Task<List<ReadTestResultDto>> GetByRecordIdAsync(int recordId, CancellationToken ct = default)
        {
            var list = await _repo.GetEntitiesByRecordIdAsync(recordId, ct);
            return list.Select(MapToDto).ToList();
        }

        public async Task<ReadTestResultDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _repo.GetEntityByIdAsync(id, ct);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<List<TestTypeLite>> GetTestTypesAsync(CancellationToken ct = default)
        {
            var entities = await _repo.GetTestTypeEntitiesAsync(ct);
            return entities.Select(s => new TestTypeLite
            {
                TestTypeId = s.ServiceId,
                TestName = s.ServiceName
            }).ToList();
        }

        public async Task<PagedResult<TestWorklistItemDto>> GetWorklistAsync(
            TestWorklistQueryDto query,
            CancellationToken ct = default)
        {
            var testTypes = await _repo.GetTestTypeEntitiesAsync(ct);
            var requiredIds = testTypes.Select(t => t.ServiceId).ToList();

            var records = await _repo.GetWorklistEntitiesAsync(
                query.VisitDate,
                query.PatientName,
                ct);

            if (query.RequiredState == RequiredState.Missing)
            {
                records = records
                    .Where(r => !requiredIds.All(tid => r.TestResults.Any(tr => tr.ServiceId == tid)))
                    .ToList();
            }
            else if (query.RequiredState == RequiredState.Complete)
            {
                records = records
                    .Where(r => requiredIds.All(tid => r.TestResults.Any(tr => tr.ServiceId == tid)))
                    .ToList();
            }

            var items = new List<TestWorklistItemDto>();

            foreach (var r in records)
            {
                items.Add(new TestWorklistItemDto
                {
                    RecordId = r.RecordId,
                    AppointmentId = r.AppointmentId,
                    AppointmentDate = r.Appointment.AppointmentDate,
                    PatientId = r.Appointment.PatientId,
                    PatientName = r.Appointment.Patient.User.FullName,
                    HasAllRequiredResults = requiredIds.All(tid => r.TestResults.Any(tr => tr.ServiceId == tid)),
                    Results = r.TestResults.Select(MapToDto).ToList()
                });
            }

            var pagedItems = items
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            return new PagedResult<TestWorklistItemDto>
            {
                Items = pagedItems,
                TotalCount = items.Count,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        #region Private validations

        private void ValidateCreate(CreateTestResultDto dto)
        {
            var errors = new List<string>();

            if (dto.RecordId <= 0)
                errors.Add("Mã phiếu khám không hợp lệ.");

            if (dto.TestTypeId <= 0)
                errors.Add("Mã loại xét nghiệm không hợp lệ.");

            if (string.IsNullOrWhiteSpace(dto.ResultValue))
                errors.Add("Giá trị kết quả xét nghiệm không được để trống.");

            if (errors.Count > 0)
            {
                throw new ArgumentException(string.Join(" ", errors));
            }
        }

        private void ValidateUpdate(UpdateTestResultDto dto)
        {
            var errors = new List<string>();

            if (dto.ResultValue != null && string.IsNullOrWhiteSpace(dto.ResultValue))
                errors.Add("Giá trị kết quả xét nghiệm không được để trống nếu cập nhật.");

            if (errors.Count > 0)
            {
                throw new ArgumentException(string.Join(" ", errors));
            }
        }

        #endregion
    }
}

