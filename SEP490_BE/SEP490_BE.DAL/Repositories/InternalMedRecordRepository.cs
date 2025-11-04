using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories
{
    public class InternalMedRecordRepository: IInternalMedRecordRepository
    {
        private readonly DiamondHealthContext _db;
        public InternalMedRecordRepository(DiamondHealthContext db) => _db = db;

        public Task<bool> MedicalRecordExistsAsync(int recordId, CancellationToken ct = default)
            => _db.MedicalRecords.AnyAsync(r => r.RecordId == recordId, ct);

        public Task<InternalMedRecord?> GetByRecordIdAsync(int recordId, CancellationToken ct = default)
            => _db.InternalMedRecords.FirstOrDefaultAsync(x => x.RecordId == recordId, ct);

        public Task<bool> HasInternalMedAsync(int recordId, CancellationToken ct = default)
            => _db.InternalMedRecords.AnyAsync(x => x.RecordId == recordId, ct);

        public Task<bool> HasPediatricAsync(int recordId, CancellationToken ct = default)
            => _db.PediatricRecords.AnyAsync(x => x.RecordId == recordId, ct);

        public async Task<InternalMedRecord> CreateAsync(InternalMedRecord entity, CancellationToken ct = default)
        {
            _db.InternalMedRecords.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task UpdateAsync(InternalMedRecord entity, CancellationToken ct = default)
        {
            _db.InternalMedRecords.Update(entity);
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int recordId, CancellationToken ct = default)
        {
            var entity = await _db.InternalMedRecords.FirstOrDefaultAsync(x => x.RecordId == recordId, ct)
                ?? throw new KeyNotFoundException($"InternalMedRecord for RecordId {recordId} not found");
            _db.InternalMedRecords.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }
}
