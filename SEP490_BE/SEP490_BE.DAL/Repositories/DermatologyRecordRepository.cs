using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories
{
    public class DermatologyRecordRepository : IDermatologyRecordRepository
    {
        private readonly DiamondHealthContext _context;

        public DermatologyRecordRepository(DiamondHealthContext context)
        {
            _context = context;
        }

        public Task<DermatologyRecord?> GetByRecordIdAsync(int recordId, CancellationToken ct = default)
        {
            return _context.DermatologyRecords
                .FirstOrDefaultAsync(x => x.RecordId == recordId, ct);
        }

        public Task<bool> HasDermatologyAsync(int recordId, CancellationToken ct = default)
        {
            return _context.DermatologyRecords
                .AnyAsync(x => x.RecordId == recordId, ct);
        }

        public async Task<DermatologyRecord> CreateAsync(DermatologyRecord entity, CancellationToken ct = default)
        {
            _context.DermatologyRecords.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task UpdateAsync(DermatologyRecord entity, CancellationToken ct = default)
        {
            _context.DermatologyRecords.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int recordId, CancellationToken ct = default)
        {
            var entity = await _context.DermatologyRecords
                .FirstOrDefaultAsync(x => x.RecordId == recordId, ct);

            if (entity != null)
            {
                _context.DermatologyRecords.Remove(entity);
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}
