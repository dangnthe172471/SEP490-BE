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

        public async Task<ReadTestResultDto> CreateAsync(CreateTestResultDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.ResultValue))
                throw new ArgumentException("ResultValue không được rỗng");

            if (!await _repo.RecordExistsAsync(dto.RecordId, ct))
                throw new KeyNotFoundException($"MedicalRecord {dto.RecordId} không tồn tại");

            if (!await _repo.TestTypeExistsAsync(dto.TestTypeId, ct))
                throw new KeyNotFoundException($"TestType {dto.TestTypeId} không tồn tại hoặc không phải loại xét nghiệm");

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

            // Giao cho repository xử lý tạo MedicalService
            await _repo.EnsureMedicalServiceForTestAsync(dto.RecordId, dto.TestTypeId, ct);

            var read = await _repo.GetByIdAsync(created.TestResultId, ct)
                ?? throw new InvalidOperationException("Không đọc được kết quả vừa tạo");
            return read;
        }

        public async Task<ReadTestResultDto> UpdateAsync(int testResultId, UpdateTestResultDto dto, CancellationToken ct = default)
        {
            var entity = await _repo.GetEntityByIdAsync(testResultId, ct)
                ?? throw new KeyNotFoundException($"TestResult {testResultId} không tồn tại");

            if (dto.ResultValue != null)
            {
                if (string.IsNullOrWhiteSpace(dto.ResultValue))
                    throw new ArgumentException("ResultValue không được rỗng");
                entity.ResultValue = dto.ResultValue.Trim();
            }
            if (dto.Unit != null) entity.Unit = dto.Unit.Trim();
            if (dto.Attachment != null) entity.Attachment = dto.Attachment.Trim();
            if (dto.ResultDate.HasValue) entity.ResultDate = dto.ResultDate;
            if (dto.Notes != null) entity.Notes = dto.Notes.Trim();

            await _repo.UpdateAsync(entity, ct);
            var read = await _repo.GetByIdAsync(entity.TestResultId, ct)
                ?? throw new InvalidOperationException("Không đọc được kết quả sau cập nhật");
            return read;
        }

        public Task DeleteAsync(int testResultId, CancellationToken ct = default)
            => _repo.DeleteAsync(testResultId, ct);

        public Task<List<ReadTestResultDto>> GetByRecordIdAsync(int recordId, CancellationToken ct = default)
            => _repo.GetByRecordIdAsync(recordId, ct);

        public Task<ReadTestResultDto?> GetByIdAsync(int id, CancellationToken ct = default)
            => _repo.GetByIdAsync(id, ct);

        public Task<PagedResult<TestWorklistItemDto>> GetWorklistAsync(TestWorklistQueryDto query, CancellationToken ct = default)
        {
            var page = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var size = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 200);

            return _repo.GetWorklistAsync(
                query.VisitDate,
                query.PatientName,
                page,
                size,
                query.RequiredState,
                ct);
        }

        public Task<List<TestTypeLite>> GetTestTypesAsync(CancellationToken ct = default)
            => _repo.GetTestTypesAsync(ct);

    }
}
