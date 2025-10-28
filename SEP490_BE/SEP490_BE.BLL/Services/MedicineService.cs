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

        public async Task<int?> GetProviderIdByUserIdAsync(int userId, CancellationToken ct = default)
        => await _medicineRepository.GetProviderIdByUserIdAsync(userId, ct);

        public async Task<List<ReadMedicineDto>> GetAllMedicineAsync(CancellationToken cancellationToken = default)
        {
            var medicines = await _medicineRepository.GetAllMedicineAsync(cancellationToken);

            return medicines.Select(m => new ReadMedicineDto
            {
                MedicineId = m.MedicineId,
                MedicineName = m.MedicineName,
                SideEffects = m.SideEffects,
                Status = m.Status,
                ProviderId = m.ProviderId,
                ProviderName = m.Provider?.User.FullName
            }).ToList();
        }

        public async Task<ReadMedicineDto?> GetMedicineByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var medicine = await _medicineRepository.GetMedicineByIdAsync(id, cancellationToken);

            if (medicine == null) return null;

            return new ReadMedicineDto
            {
                MedicineId = medicine.MedicineId,
                MedicineName = medicine.MedicineName,
                SideEffects = medicine.SideEffects,
                Status = medicine.Status,
                ProviderId = medicine.ProviderId,
                ProviderName = medicine.Provider?.User.FullName
            };
        }

        public async Task CreateMedicineAsync(CreateMedicineDto dto, int providerId, CancellationToken cancellationToken = default)
        {
            var name = dto.MedicineName?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Medicine name is required.");



            var newMedicine = new Medicine
            {
                MedicineName = name,
                ProviderId = providerId,
                SideEffects = dto.SideEffects,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Providing" : dto.Status!.Trim()
            };

            await _medicineRepository.CreateMedicineAsync(newMedicine, cancellationToken);
        }


        public async Task UpdateMedicineAsync(int id, UpdateMedicineDto dto, CancellationToken cancellationToken = default)
        {
            var existingMedicine = await _medicineRepository.GetMedicineByIdAsync(id, cancellationToken);

            if (existingMedicine == null)
                throw new KeyNotFoundException($"Medicine with ID {id} not found.");
            if (dto.MedicineName != null && string.IsNullOrWhiteSpace(dto.MedicineName))
                throw new ArgumentException("Medicine name cannot be empty or whitespace.");

            existingMedicine.MedicineName = dto.MedicineName?.Trim() ?? existingMedicine.MedicineName;
            existingMedicine.SideEffects = dto.SideEffects ?? existingMedicine.SideEffects;
            existingMedicine.Status = dto.Status ?? existingMedicine.Status;

            await _medicineRepository.UpdateMedicineAsync(existingMedicine, cancellationToken);
        }

        public async Task<List<ReadMedicineDto>> GetByProviderIdAsync(int providerId, CancellationToken cancellationToken = default)
        {
            var medicines = await _medicineRepository.GetByProviderIdAsync(providerId, cancellationToken);

            return medicines.Select(m => new ReadMedicineDto
            {
                MedicineId = m.MedicineId,
                MedicineName = m.MedicineName,
                SideEffects = m.SideEffects,
                Status = m.Status,
                ProviderId = m.ProviderId,
                ProviderName = m.Provider?.User.FullName
            }).ToList();
        }

        public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await _medicineRepository.SoftDeleteAsync(id, cancellationToken);
        }

        public async Task<List<ReadMedicineDto>> GetMineAsync(int userId, CancellationToken ct = default)
        {
            var providerId = await _medicineRepository.GetProviderIdByUserIdAsync(userId, ct);
            if (!providerId.HasValue)
                throw new InvalidOperationException("Người dùng hiện tại không phải là nhà cung cấp.");

            var medicines = await _medicineRepository.GetByProviderIdAsync(providerId.Value, ct);

            return medicines.Select(m => new ReadMedicineDto
            {
                MedicineId = m.MedicineId,
                MedicineName = m.MedicineName,
                SideEffects = m.SideEffects,
                Status = m.Status,
                ProviderId = m.ProviderId,
                ProviderName = m.Provider?.User.FullName
            }).ToList();
        }

        public async Task<PagedResult<ReadMedicineDto>> GetMinePagedAsync(
            int userId, int pageNumber, int pageSize, string? status = null, string? sort = null, CancellationToken ct = default)
        {
            var providerId = await _medicineRepository.GetProviderIdByUserIdAsync(userId, ct);
            if (!providerId.HasValue)
                throw new InvalidOperationException("Người dùng hiện tại không phải là nhà cung cấp.");

            if (pageSize > 100) pageSize = 100;
            if (pageNumber < 1) pageNumber = 1;

            string? normStatus = status?.Trim();
            if (!string.IsNullOrEmpty(normStatus))
            {
                normStatus = normStatus.Equals("providing", StringComparison.OrdinalIgnoreCase) ? "Providing"
                           : normStatus.Equals("stopped", StringComparison.OrdinalIgnoreCase) ? "Stopped"
                           : null;
            }

            string? normSort = sort?.Trim().ToLowerInvariant();
            if (normSort != "az" && normSort != "za") normSort = null;

            var (items, totalCount) = await _medicineRepository
                .GetByProviderIdPagedAsync(providerId.Value, pageNumber, pageSize, normStatus, normSort, ct);

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
                TotalCount = totalCount
            };
        }

    }
}
