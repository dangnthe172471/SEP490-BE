using SEP490_BE.BLL.IServices;
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

        public async Task<int?> GetProviderIdByUserIdAsync(int userId, CancellationToken ct = default)
            => await _medicineRepository.GetProviderIdByUserIdAsync(userId, ct);

        public async Task<List<ReadMedicineDto>> GetAllMedicineAsync(CancellationToken ct = default)
        {
            var meds = await _medicineRepository.GetAllMedicineAsync(ct);
            return meds.Select(m => new ReadMedicineDto
            {
                MedicineId = m.MedicineId,
                MedicineName = m.MedicineName,
                SideEffects = m.SideEffects,
                Status = m.Status,
                ProviderId = m.ProviderId,
                ProviderName = m.Provider?.User.FullName
            }).ToList();
        }

        public async Task<ReadMedicineDto?> GetMedicineByIdAsync(int id, CancellationToken ct = default)
        {
            var m = await _medicineRepository.GetMedicineByIdAsync(id, ct);
            return m == null ? null : new ReadMedicineDto
            {
                MedicineId = m.MedicineId,
                MedicineName = m.MedicineName,
                SideEffects = m.SideEffects,
                Status = m.Status,
                ProviderId = m.ProviderId,
                ProviderName = m.Provider?.User.FullName
            };
        }

        public async Task CreateMedicineAsync(CreateMedicineDto dto, int providerId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.MedicineName))
                throw new ArgumentException("Medicine name is required.", nameof(dto.MedicineName));

            var medicine = new Medicine
            {
                MedicineName = NormalizeName(dto.MedicineName),
                ProviderId = providerId,
                SideEffects = dto.SideEffects,
                Status = NormalizeStatusOrDefault(dto.Status)
            };

            await _medicineRepository.CreateMedicineAsync(medicine, ct);
        }

        public async Task UpdateMineAsync(int userId, int id, UpdateMedicineDto dto, CancellationToken ct = default)
        {
            var providerId = await _medicineRepository.GetProviderIdByUserIdAsync(userId, ct);
            if (!providerId.HasValue)
                throw new UnauthorizedAccessException("Current user is not a provider.");

            var existing = await _medicineRepository.GetMedicineByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Medicine with ID {id} not found.");

            if (existing.ProviderId != providerId.Value)
                throw new UnauthorizedAccessException("You are not allowed to update this medicine.");

            var newName = dto.MedicineName is null ? existing.MedicineName : NormalizeName(dto.MedicineName);
            if (dto.MedicineName != null && string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Medicine name cannot be empty or whitespace.", nameof(dto.MedicineName));

            var updateEntity = new Medicine
            {
                MedicineId = id,
                ProviderId = existing.ProviderId,
                MedicineName = newName,
                SideEffects = dto.SideEffects ?? existing.SideEffects,
                Status = dto.Status != null ? NormalizeStatusOrDefault(dto.Status) : existing.Status
            };
            await _medicineRepository.UpdateMedicineAsync(updateEntity, ct);
        }

        public async Task<PagedResult<ReadMedicineDto>> GetMinePagedAsync(
            int userId, int pageNumber, int pageSize, string? status = null, string? sort = null, CancellationToken ct = default)
        {
            var providerId = await _medicineRepository.GetProviderIdByUserIdAsync(userId, ct);
            if (!providerId.HasValue)
                throw new UnauthorizedAccessException("Current user is not a provider.");

            if (pageSize > 100) pageSize = 100;
            if (pageNumber < 1) pageNumber = 1;

            string? normalizedStatus = string.IsNullOrWhiteSpace(status) ? null : NormalizeStatusOrDefault(status);

            var (items, total) = await _medicineRepository.GetByProviderIdPagedAsync(
                providerId.Value, pageNumber, pageSize, normalizedStatus, sort, ct);

            var mapped = items.Select(m => new ReadMedicineDto
            {
                MedicineId = m.MedicineId,
                MedicineName = m.MedicineName,
                SideEffects = m.SideEffects,
                Status = m.Status,
                ProviderId = m.ProviderId,
                ProviderName = m.Provider?.User.FullName
            }).ToList();

            return new PagedResult<ReadMedicineDto>
            {
                Items = mapped,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total
            };
        }
    }
}
