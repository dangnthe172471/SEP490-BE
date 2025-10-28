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

        public async Task<List<Medicine>> GetAllMedicineAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Medicines
                .Include(m => m.Provider)
                .ThenInclude(p => p.User)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Medicine?> GetMedicineByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Medicines
                .Include(m => m.Provider)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(m => m.MedicineId == id, cancellationToken);
        }

        public async Task CreateMedicineAsync(Medicine medicine, CancellationToken cancellationToken = default)
        {
            if (medicine == null)
                throw new ArgumentNullException(nameof(medicine), "Medicine object cannot be null.");

            if (string.IsNullOrWhiteSpace(medicine.MedicineName))
                throw new ArgumentException("Medicine name cannot be empty or whitespace.", nameof(medicine.MedicineName));

            medicine.MedicineName = medicine.MedicineName.Trim();

            var exists = await _dbContext.Medicines.AnyAsync(
                m => m.ProviderId == medicine.ProviderId && m.MedicineName == medicine.MedicineName,
                cancellationToken);

            if (exists)
                throw new InvalidOperationException($"Medicine '{medicine.MedicineName}' already exists for this provider.");

            await _dbContext.Medicines.AddAsync(medicine, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateMedicineAsync(Medicine medicine, CancellationToken cancellationToken = default)
        {
            var existingMedicine = await _dbContext.Medicines
                .AsTracking()
                .FirstOrDefaultAsync(m => m.MedicineId == medicine.MedicineId, cancellationToken);

            if (existingMedicine == null)
                throw new KeyNotFoundException($"Medicine with ID {medicine.MedicineId} not found.");

            if (existingMedicine.ProviderId != medicine.ProviderId)
                throw new InvalidOperationException("Changing Provider is not allowed.");

            if (existingMedicine.MedicineName != null && string.IsNullOrWhiteSpace(existingMedicine.MedicineName))
                throw new ArgumentException("Medicine name cannot be empty or whitespace.");

            existingMedicine.MedicineName = medicine.MedicineName?.Trim() ?? existingMedicine.MedicineName;
            existingMedicine.SideEffects = medicine.SideEffects;
            existingMedicine.Status = medicine.Status;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Medicine>> GetByProviderIdAsync(int providerId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Medicines
                .Include(m => m.Provider)
                .ThenInclude(p => p.User)
                .AsNoTracking()
                .Where(m => m.ProviderId == providerId)
                .ToListAsync(cancellationToken);
        }

        public async Task SoftDeleteAsync(int medicineId, CancellationToken cancellationToken = default)
        {
            var medicine = await _dbContext.Medicines
                .FirstOrDefaultAsync(m => m.MedicineId == medicineId, cancellationToken);

            if (medicine == null)
                throw new KeyNotFoundException($"Medicine with ID {medicineId} not found.");

            medicine.Status = "Stopped";
            await _dbContext.SaveChangesAsync(cancellationToken);
        }


        public async Task<PharmacyProvider?> GetByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return await _dbContext.PharmacyProviders
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId, ct);
        }

        public async Task<int?> GetProviderIdByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return await _dbContext.PharmacyProviders
                .Where(p => p.UserId == userId)
                .Select(p => (int?)p.ProviderId)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<(List<Medicine> Items, int TotalCount)> GetByProviderIdPagedAsync(
            int providerId, int pageNumber, int pageSize, string? status = null, string? sort = null, CancellationToken ct = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var baseQuery = _dbContext.Medicines
                .Where(m => m.ProviderId == providerId)
                .Include(m => m.Provider)
                    .ThenInclude(p => p.User)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status))
            {
                baseQuery = baseQuery.Where(m => m.Status != null && m.Status == status);
            }

            IOrderedQueryable<Medicine> ordered;
            if (string.Equals(sort, "az", StringComparison.OrdinalIgnoreCase))
            {
                ordered = baseQuery.OrderBy(m => m.MedicineName);
            }
            else if (string.Equals(sort, "za", StringComparison.OrdinalIgnoreCase))
            {
                ordered = baseQuery.OrderByDescending(m => m.MedicineName);
            }
            else
            {
                ordered = baseQuery.OrderByDescending(m => m.MedicineId);
            }

            var totalCount = await baseQuery.CountAsync(ct);

            var items = await ordered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}
