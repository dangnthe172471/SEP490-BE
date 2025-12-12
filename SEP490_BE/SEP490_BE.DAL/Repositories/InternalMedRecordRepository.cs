using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories
{
    public class InternalMedRecordRepository : IInternalMedRecordRepository
    {
        private readonly DiamondHealthContext _context;

        public InternalMedRecordRepository(DiamondHealthContext context)
        {
            _context = context;
        }

        public Task<InternalMedRecord?> GetByRecordIdAsync(int recordId, CancellationToken ct = default)
        {
            return _context.InternalMedRecords
                .FirstOrDefaultAsync(x => x.RecordId == recordId, ct);
        }

        public Task<bool> HasInternalMedAsync(int recordId, CancellationToken ct = default)
        {
            return _context.InternalMedRecords
                .AnyAsync(x => x.RecordId == recordId, ct);
        }

        public async Task<InternalMedRecord> CreateAsync(InternalMedRecord entity, CancellationToken ct = default)
        {
            _context.InternalMedRecords.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task UpdateAsync(InternalMedRecord entity, CancellationToken ct = default)
        {
            _context.InternalMedRecords.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int recordId, CancellationToken ct = default)
        {
            var entity = await _context.InternalMedRecords
                .FirstOrDefaultAsync(x => x.RecordId == recordId, ct);

            if (entity != null)
            {
                _context.InternalMedRecords.Remove(entity);
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}
