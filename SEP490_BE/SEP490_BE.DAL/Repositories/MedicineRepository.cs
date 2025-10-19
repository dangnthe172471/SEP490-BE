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

        public async Task<List<Medicine>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Medicines
                .Include(m => m.Provider)
                .ThenInclude(p => p.User)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Medicine?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Medicines
                .Include(m => m.Provider)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(m => m.MedicineId == id, cancellationToken);
        }

        public async Task CreateAsync(Medicine medicine, CancellationToken cancellationToken = default)
        {
            medicine.MedicineName = medicine.MedicineName?.Trim();

            var exists = await _dbContext.Medicines.AnyAsync(
                m => m.ProviderId == medicine.ProviderId && m.MedicineName == medicine.MedicineName,
                cancellationToken);

            if (exists)
                throw new InvalidOperationException($"Medicine '{medicine.MedicineName}' already exists for this provider.");

            await _dbContext.Medicines.AddAsync(medicine, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }


        public async Task UpdateAsync(Medicine medicine, CancellationToken cancellationToken = default)
        {
            var existingMedicine = await _dbContext.Medicines
                .AsTracking()
                .FirstOrDefaultAsync(m => m.MedicineId == medicine.MedicineId, cancellationToken);

            if (existingMedicine == null)
                throw new KeyNotFoundException($"Medicine with ID {medicine.MedicineId} not found.");

            if (existingMedicine.ProviderId != medicine.ProviderId)
                throw new InvalidOperationException("Changing Provider is not allowed.");

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

            medicine.Status = "Inactive";
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
    }
}
