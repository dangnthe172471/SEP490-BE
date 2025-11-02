using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories
{
    public class PediatricRecordRepository : IPediatricRecordRepository
    {
        private readonly DiamondHealthContext _db;
        public PediatricRecordRepository(DiamondHealthContext db) => _db = db;

        public Task<bool> MedicalRecordExistsAsync(int recordId, CancellationToken ct = default)
            => _db.MedicalRecords.AnyAsync(r => r.RecordId == recordId, ct);

        public Task<PediatricRecord?> GetByRecordIdAsync(int recordId, CancellationToken ct = default)
            => _db.PediatricRecords.FirstOrDefaultAsync(x => x.RecordId == recordId, ct);

        public Task<bool> HasPediatricAsync(int recordId, CancellationToken ct = default)
            => _db.PediatricRecords.AnyAsync(x => x.RecordId == recordId, ct);

        public Task<bool> HasInternalMedAsync(int recordId, CancellationToken ct = default)
            => _db.InternalMedRecords.AnyAsync(x => x.RecordId == recordId, ct);

        public async Task<PediatricRecord> CreateAsync(PediatricRecord entity, CancellationToken ct = default)
        {
            _db.PediatricRecords.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task UpdateAsync(PediatricRecord entity, CancellationToken ct = default)
        {
            _db.PediatricRecords.Update(entity);
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int recordId, CancellationToken ct = default)
        {
            var entity = await _db.PediatricRecords.FirstOrDefaultAsync(x => x.RecordId == recordId, ct)
                ?? throw new KeyNotFoundException($"PediatricRecord for RecordId {recordId} not found");
            _db.PediatricRecords.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }
}
