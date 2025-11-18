using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories
{
    public class PediatricRecordRepository : IPediatricRecordRepository
    {
        private readonly DiamondHealthContext _context;

        public PediatricRecordRepository(DiamondHealthContext context)
        {
            _context = context;
        }

        public Task<PediatricRecord?> GetByRecordIdAsync(int recordId, CancellationToken ct = default)
        {
            return _context.PediatricRecords
                .FirstOrDefaultAsync(x => x.RecordId == recordId, ct);
        }

        public Task<bool> HasPediatricAsync(int recordId, CancellationToken ct = default)
        {
            return _context.PediatricRecords
                .AnyAsync(x => x.RecordId == recordId, ct);
        }

        public async Task<PediatricRecord> CreateAsync(PediatricRecord entity, CancellationToken ct = default)
        {
            _context.PediatricRecords.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task UpdateAsync(PediatricRecord entity, CancellationToken ct = default)
        {
            _context.PediatricRecords.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int recordId, CancellationToken ct = default)
        {
            var entity = await _context.PediatricRecords
                .FirstOrDefaultAsync(x => x.RecordId == recordId, ct);

            if (entity != null)
            {
                _context.PediatricRecords.Remove(entity);
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}
