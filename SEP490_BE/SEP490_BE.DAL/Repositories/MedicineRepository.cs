using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories
{
    public class MedicineRepository : IMedicineRepository
    {
        private readonly DiamondHealthContext _dbContext;

        public MedicineRepository(DiamondHealthContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Medicine>> GetAllMedicineAsync(CancellationToken ct = default)
        {
            return await _dbContext.Medicines
                .Include(m => m.Provider).ThenInclude(p => p.User)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<Medicine?> GetMedicineByIdAsync(int id, CancellationToken ct = default)
        {
            return await _dbContext.Medicines
                .Include(m => m.Provider).ThenInclude(p => p.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MedicineId == id, ct);
        }

        public async Task CreateMedicineAsync(Medicine medicine, CancellationToken ct = default)
        {
            var normalized = (medicine.MedicineName ?? string.Empty).Trim().ToLower();

            bool exists = await _dbContext.Medicines.AnyAsync(
                m => m.ProviderId == medicine.ProviderId &&
                     m.MedicineName.ToLower() == normalized,
                ct);

            if (exists)
                throw new InvalidOperationException(
                    $"Medicine '{medicine.MedicineName}' already exists for this provider.");

            await _dbContext.Medicines.AddAsync(medicine, ct);
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task UpdateMedicineAsync(Medicine medicine, CancellationToken ct = default)
        {
            var existing = await _dbContext.Medicines
                .FirstOrDefaultAsync(m => m.MedicineId == medicine.MedicineId, ct);

            if (existing == null)
                throw new KeyNotFoundException($"Medicine with ID {medicine.MedicineId} not found.");

            // Không cho đổi Provider
            if (existing.ProviderId != medicine.ProviderId)
                throw new InvalidOperationException("Changing Provider is not allowed.");

            // Xử lý đổi tên + check trùng
            var incomingName = medicine.MedicineName?.Trim();
            var isRename = incomingName != null &&
                           !string.Equals(incomingName, existing.MedicineName,
                               StringComparison.OrdinalIgnoreCase);

            if (isRename)
            {
                var normalized = incomingName!.ToLower();
                bool duplicated = await _dbContext.Medicines.AnyAsync(
                    m => m.ProviderId == existing.ProviderId &&
                         m.MedicineId != existing.MedicineId &&
                         m.MedicineName.ToLower() == normalized,
                    ct);

                if (duplicated)
                    throw new InvalidOperationException(
                        $"Medicine '{incomingName}' already exists for this provider.");

                existing.MedicineName = incomingName; // đã trim
            }

            // 🔥 BUG FIX: cập nhật đầy đủ các trường còn lại
            existing.Status = medicine.Status;
            existing.ActiveIngredient = medicine.ActiveIngredient;
            existing.Strength = medicine.Strength;
            existing.DosageForm = medicine.DosageForm;
            existing.Route = medicine.Route;
            existing.PrescriptionUnit = medicine.PrescriptionUnit;
            existing.TherapeuticClass = medicine.TherapeuticClass;
            existing.PackSize = medicine.PackSize;
            existing.CommonSideEffects = medicine.CommonSideEffects;
            existing.NoteForDoctor = medicine.NoteForDoctor;

            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<int?> GetProviderIdByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return await _dbContext.PharmacyProviders
                .Where(p => p.UserId == userId)
                .Select(p => (int?)p.ProviderId)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<(List<Medicine> Items, int TotalCount)> GetByProviderIdPagedAsync(
            int providerId, int pageNumber, int pageSize,
            string? status = null, string? sort = null,
            CancellationToken ct = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _dbContext.Medicines
                .Where(m => m.ProviderId == providerId)
                .Include(m => m.Provider).ThenInclude(p => p.User)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status))
            {
                var s = status.Trim().ToLower();
                query = query.Where(m => m.Status != null && m.Status.ToLower() == s);
            }

            IOrderedQueryable<Medicine> ordered = sort?.ToLower() switch
            {
                "az" => query.OrderBy(m => m.MedicineName),
                "za" => query.OrderByDescending(m => m.MedicineName),
                _ => query.OrderByDescending(m => m.MedicineId)
            };

            int total = await query.CountAsync(ct);
            var items = await ordered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }
    }
}
