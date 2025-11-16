using ClosedXML.Excel;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.MedicineDTO;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.BLL.Services
{
    public class MedicineService : IMedicineService
    {
        private readonly IMedicineRepository _medicineRepository;

        public MedicineService(IMedicineRepository medicineRepository)
        {
            _medicineRepository = medicineRepository;
        }

        private static string NormalizeName(string name) => name.Trim();

        private static string NormalizeStatusOrDefault(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "Providing";
            var s = raw.Trim();
            if (s.Equals("Providing", StringComparison.OrdinalIgnoreCase)) return "Providing";
            if (s.Equals("Stopped", StringComparison.OrdinalIgnoreCase)) return "Stopped";
            throw new ArgumentException("Invalid status. Allowed: Providing | Stopped.", nameof(raw));
        }

        private static void ValidateMaxLength(string? value, int max, string fieldName)
        {
            if (value != null && value.Length > max)
                throw new ArgumentException($"{fieldName} cannot exceed {max} characters.");
        }

        private static void EnsureRequired(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{fieldName} is required.");
        }

        // CREATE: bắt buộc nhập đầy đủ các trường chính
        private static void ValidateCreateDto(CreateMedicineDto dto)
        {
            EnsureRequired(dto.MedicineName, nameof(dto.MedicineName));
            EnsureRequired(dto.ActiveIngredient, nameof(dto.ActiveIngredient));
            EnsureRequired(dto.Strength, nameof(dto.Strength));
            EnsureRequired(dto.DosageForm, nameof(dto.DosageForm));
            EnsureRequired(dto.Route, nameof(dto.Route));
            EnsureRequired(dto.PrescriptionUnit, nameof(dto.PrescriptionUnit));
            EnsureRequired(dto.TherapeuticClass, nameof(dto.TherapeuticClass));
            EnsureRequired(dto.PackSize, nameof(dto.PackSize));
            // Status có thể để trống, NormalizeStatusOrDefault sẽ default "Providing"

            ValidateMaxLength(dto.MedicineName, 200, nameof(dto.MedicineName));
            ValidateMaxLength(dto.ActiveIngredient, 200, nameof(dto.ActiveIngredient));
            ValidateMaxLength(dto.Strength, 50, nameof(dto.Strength));
            ValidateMaxLength(dto.DosageForm, 100, nameof(dto.DosageForm));
            ValidateMaxLength(dto.Route, 50, nameof(dto.Route));
            ValidateMaxLength(dto.PrescriptionUnit, 50, nameof(dto.PrescriptionUnit));
            ValidateMaxLength(dto.TherapeuticClass, 100, nameof(dto.TherapeuticClass));
            ValidateMaxLength(dto.PackSize, 100, nameof(dto.PackSize));
            ValidateMaxLength(dto.NoteForDoctor, 500, nameof(dto.NoteForDoctor));
            ValidateMaxLength(dto.Status, 20, nameof(dto.Status));
        }

        // UPDATE: không bắt buộc, chỉ kiểm tra độ dài nếu có gửi
        private static void ValidateUpdateDto(UpdateMedicineDto dto)
        {
            if (dto.MedicineName != null)
                ValidateMaxLength(dto.MedicineName, 200, nameof(dto.MedicineName));

            ValidateMaxLength(dto.ActiveIngredient, 200, nameof(dto.ActiveIngredient));
            ValidateMaxLength(dto.Strength, 50, nameof(dto.Strength));
            ValidateMaxLength(dto.DosageForm, 100, nameof(dto.DosageForm));
            ValidateMaxLength(dto.Route, 50, nameof(dto.Route));
            ValidateMaxLength(dto.PrescriptionUnit, 50, nameof(dto.PrescriptionUnit));
            ValidateMaxLength(dto.TherapeuticClass, 100, nameof(dto.TherapeuticClass));
            ValidateMaxLength(dto.PackSize, 100, nameof(dto.PackSize));
            ValidateMaxLength(dto.NoteForDoctor, 500, nameof(dto.NoteForDoctor));
            ValidateMaxLength(dto.Status, 20, nameof(dto.Status));
        }

        public async Task<int?> GetProviderIdByUserIdAsync(int userId, CancellationToken ct = default)
            => await _medicineRepository.GetProviderIdByUserIdAsync(userId, ct);

        public async Task<List<ReadMedicineDto>> GetAllMedicineAsync(CancellationToken ct = default)
        {
            var meds = await _medicineRepository.GetAllMedicineAsync(ct);

            return meds.Select(m => new ReadMedicineDto
            {
                MedicineId = m.MedicineId,
                MedicineName = m.MedicineName,
                Status = m.Status,
                ProviderId = m.ProviderId,
                ProviderName = m.Provider?.User.FullName,
                ActiveIngredient = m.ActiveIngredient,
                Strength = m.Strength,
                DosageForm = m.DosageForm,
                Route = m.Route,
                PrescriptionUnit = m.PrescriptionUnit,
                TherapeuticClass = m.TherapeuticClass,
                PackSize = m.PackSize,
                CommonSideEffects = m.CommonSideEffects,
                NoteForDoctor = m.NoteForDoctor
            }).ToList();
        }

        public async Task<ReadMedicineDto?> GetMedicineByIdAsync(int id, CancellationToken ct = default)
        {
            var m = await _medicineRepository.GetMedicineByIdAsync(id, ct);
            if (m == null) return null;

            return new ReadMedicineDto
            {
                MedicineId = m.MedicineId,
                MedicineName = m.MedicineName,
                Status = m.Status,
                ProviderId = m.ProviderId,
                ProviderName = m.Provider?.User.FullName,
                ActiveIngredient = m.ActiveIngredient,
                Strength = m.Strength,
                DosageForm = m.DosageForm,
                Route = m.Route,
                PrescriptionUnit = m.PrescriptionUnit,
                TherapeuticClass = m.TherapeuticClass,
                PackSize = m.PackSize,
                CommonSideEffects = m.CommonSideEffects,
                NoteForDoctor = m.NoteForDoctor
            };
        }

        public async Task CreateMedicineAsync(CreateMedicineDto dto, int providerId, CancellationToken ct = default)
        {
            ValidateCreateDto(dto);

            var medicine = new Medicine
            {
                MedicineName = NormalizeName(dto.MedicineName),
                ProviderId = providerId,
                Status = NormalizeStatusOrDefault(dto.Status),

                ActiveIngredient = dto.ActiveIngredient!.Trim(),
                Strength = dto.Strength!.Trim(),
                DosageForm = dto.DosageForm!.Trim(),
                Route = dto.Route!.Trim(),
                PrescriptionUnit = dto.PrescriptionUnit!.Trim(),
                TherapeuticClass = dto.TherapeuticClass!.Trim(),
                PackSize = dto.PackSize!.Trim(),
                CommonSideEffects = dto.CommonSideEffects,
                NoteForDoctor = dto.NoteForDoctor
            };

            await _medicineRepository.CreateMedicineAsync(medicine, ct);
        }

        public async Task UpdateMineAsync(int userId, int id, UpdateMedicineDto dto, CancellationToken ct = default)
        {
            ValidateUpdateDto(dto);

            var providerId = await _medicineRepository.GetProviderIdByUserIdAsync(userId, ct);
            if (!providerId.HasValue)
                throw new UnauthorizedAccessException("Current user is not a provider.");

            var existing = await _medicineRepository.GetMedicineByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Medicine with ID {id} not found.");

            if (existing.ProviderId != providerId.Value)
                throw new UnauthorizedAccessException("You are not allowed to update this medicine.");

            var newName = dto.MedicineName is null ? existing.MedicineName : NormalizeName(dto.MedicineName);
            if (dto.MedicineName != null && string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("MedicineName cannot be empty or whitespace.");

            var updateEntity = new Medicine
            {
                MedicineId = id,
                ProviderId = existing.ProviderId,
                MedicineName = newName,
                Status = dto.Status != null ? NormalizeStatusOrDefault(dto.Status) : existing.Status,
                ActiveIngredient = dto.ActiveIngredient ?? existing.ActiveIngredient,
                Strength = dto.Strength ?? existing.Strength,
                DosageForm = dto.DosageForm ?? existing.DosageForm,
                Route = dto.Route ?? existing.Route,
                PrescriptionUnit = dto.PrescriptionUnit ?? existing.PrescriptionUnit,
                TherapeuticClass = dto.TherapeuticClass ?? existing.TherapeuticClass,
                PackSize = dto.PackSize ?? existing.PackSize,
                CommonSideEffects = dto.CommonSideEffects ?? existing.CommonSideEffects,
                NoteForDoctor = dto.NoteForDoctor ?? existing.NoteForDoctor
            };

            await _medicineRepository.UpdateMedicineAsync(updateEntity, ct);
        }

        public async Task<PagedResult<ReadMedicineDto>> GetMinePagedAsync(
            int userId, int pageNumber, int pageSize,
            string? status = null, string? sort = null,
            CancellationToken ct = default)
        {
            var providerId = await _medicineRepository.GetProviderIdByUserIdAsync(userId, ct);
            if (!providerId.HasValue)
                throw new UnauthorizedAccessException("Current user is not a provider.");

            if (pageSize > 100) pageSize = 100;
            if (pageNumber < 1) pageNumber = 1;

            string? normalizedStatus = string.IsNullOrWhiteSpace(status)
                ? null
                : NormalizeStatusOrDefault(status);

            var (items, total) = await _medicineRepository.GetByProviderIdPagedAsync(
                providerId.Value, pageNumber, pageSize, normalizedStatus, sort, ct);

            var mapped = items.Select(m => new ReadMedicineDto
            {
                MedicineId = m.MedicineId,
                MedicineName = m.MedicineName,
                Status = m.Status,
                ProviderId = m.ProviderId,
                ProviderName = m.Provider?.User.FullName,
                ActiveIngredient = m.ActiveIngredient,
                Strength = m.Strength,
                DosageForm = m.DosageForm,
                Route = m.Route,
                PrescriptionUnit = m.PrescriptionUnit,
                TherapeuticClass = m.TherapeuticClass,
                PackSize = m.PackSize,
                CommonSideEffects = m.CommonSideEffects,
                NoteForDoctor = m.NoteForDoctor
            }).ToList();

            return new PagedResult<ReadMedicineDto>
            {
                Items = mapped,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public Task<byte[]> GenerateExcelTemplateAsync(CancellationToken ct = default)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Danh sách thuốc");

            // Header tiếng Việt (giữ nguyên thứ tự cột để ImportFromExcelAsync vẫn đọc đúng)
            ws.Cell(1, 1).Value = "Tên thuốc";
            ws.Cell(1, 2).Value = "Hoạt chất chính";
            ws.Cell(1, 3).Value = "Hàm lượng";
            ws.Cell(1, 4).Value = "Dạng bào chế";
            ws.Cell(1, 5).Value = "Đường dùng";
            ws.Cell(1, 6).Value = "Đơn vị kê đơn";
            ws.Cell(1, 7).Value = "Nhóm điều trị";
            ws.Cell(1, 8).Value = "Quy cách đóng gói";
            ws.Cell(1, 9).Value = "Tác dụng phụ thường gặp";
            ws.Cell(1, 10).Value = "Ghi chú cho bác sĩ";
            ws.Cell(1, 11).Value = "Trạng thái (Providing/Stopped)";

            // Bản ghi mẫu 1
            ws.Cell(2, 1).Value = "Paracetamol DH 500";
            ws.Cell(2, 2).Value = "Paracetamol";
            ws.Cell(2, 3).Value = "500mg";
            ws.Cell(2, 4).Value = "Viên nén";
            ws.Cell(2, 5).Value = "Uống";
            ws.Cell(2, 6).Value = "Viên";
            ws.Cell(2, 7).Value = "Thuốc hạ sốt, giảm đau";
            ws.Cell(2, 8).Value = "Hộp 10 vỉ x 10 viên";
            ws.Cell(2, 9).Value = "Buồn nôn nhẹ, đau đầu";
            ws.Cell(2, 10).Value = "Không dùng quá 4g paracetamol/ngày";
            ws.Cell(2, 11).Value = "Providing";

            // Bản ghi mẫu 2
            ws.Cell(3, 1).Value = "Amoxicillin DH 500";
            ws.Cell(3, 2).Value = "Amoxicillin";
            ws.Cell(3, 3).Value = "500mg";
            ws.Cell(3, 4).Value = "Viên nang";
            ws.Cell(3, 5).Value = "Uống";
            ws.Cell(3, 6).Value = "Viên";
            ws.Cell(3, 7).Value = "Kháng sinh penicillin";
            ws.Cell(3, 8).Value = "Hộp 2 vỉ x 10 viên";
            ws.Cell(3, 9).Value = "Tiêu chảy, phát ban da";
            ws.Cell(3, 10).Value = "Thận trọng với người dị ứng penicillin";
            ws.Cell(3, 11).Value = "Providing";

            // Style header
            var headerRange = ws.Range(1, 1, 1, 11);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            var bytes = ms.ToArray();

            return Task.FromResult(bytes);
        }

        public async Task<BulkImportResultDto> ImportFromExcelAsync(
            int userId,
            Stream excelStream,
            CancellationToken ct = default)
        {
            var providerId = await _medicineRepository.GetProviderIdByUserIdAsync(userId, ct);
            if (!providerId.HasValue)
                throw new UnauthorizedAccessException("Current user is not a provider.");

            var result = new BulkImportResultDto();

            using var workbook = new XLWorkbook(excelStream);
            var ws = workbook.Worksheets.FirstOrDefault();
            if (ws == null)
                throw new ArgumentException("Excel file does not contain any worksheet.");

            var row = 2;
            while (true)
            {
                var cellName = ws.Cell(row, 1).GetString();
                var cellActive = ws.Cell(row, 2).GetString();
                var cellStrength = ws.Cell(row, 3).GetString();
                var cellDosage = ws.Cell(row, 4).GetString();
                var cellRoute = ws.Cell(row, 5).GetString();
                var cellUnit = ws.Cell(row, 6).GetString();
                var cellClass = ws.Cell(row, 7).GetString();
                var cellPack = ws.Cell(row, 8).GetString();
                var cellSideEffects = ws.Cell(row, 9).GetString();
                var cellNote = ws.Cell(row, 10).GetString();
                var cellStatus = ws.Cell(row, 11).GetString();

                bool allEmpty =
                    string.IsNullOrWhiteSpace(cellName) &&
                    string.IsNullOrWhiteSpace(cellActive) &&
                    string.IsNullOrWhiteSpace(cellStrength) &&
                    string.IsNullOrWhiteSpace(cellDosage) &&
                    string.IsNullOrWhiteSpace(cellRoute) &&
                    string.IsNullOrWhiteSpace(cellUnit) &&
                    string.IsNullOrWhiteSpace(cellClass) &&
                    string.IsNullOrWhiteSpace(cellPack) &&
                    string.IsNullOrWhiteSpace(cellSideEffects) &&
                    string.IsNullOrWhiteSpace(cellNote) &&
                    string.IsNullOrWhiteSpace(cellStatus);

                if (allEmpty)
                    break;

                result.Total++;

                try
                {
                    var dto = new CreateMedicineDto
                    {
                        MedicineName = cellName,
                        ActiveIngredient = cellActive,
                        Strength = cellStrength,
                        DosageForm = cellDosage,
                        Route = cellRoute,
                        PrescriptionUnit = cellUnit,
                        TherapeuticClass = cellClass,
                        PackSize = cellPack,
                        CommonSideEffects = string.IsNullOrWhiteSpace(cellSideEffects)
                            ? null
                            : cellSideEffects,
                        NoteForDoctor = string.IsNullOrWhiteSpace(cellNote)
                            ? null
                            : cellNote,
                        Status = string.IsNullOrWhiteSpace(cellStatus)
                            ? null
                            : cellStatus
                    };

                    await CreateMedicineAsync(dto, providerId.Value, ct);
                    result.Success++;
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add($"Row {row}: {ex.Message}");
                }

                row++;
            }

            return result;
        }
    }
}
