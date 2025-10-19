using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly DiamondHealthContext _dbContext;

        public RoomRepository(DiamondHealthContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Room>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Rooms
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Room?> GetByIdAsync(int roomId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Rooms
                .Include(r => r.Doctors)
                .FirstOrDefaultAsync(r => r.RoomId == roomId, cancellationToken);
        }

        public async Task<(List<Room> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Rooms.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r => r.RoomName.Contains(searchTerm));
            }

            // Get total count before paging
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply paging
            var items = await query
                .AsNoTracking()
                .OrderBy(r => r.RoomId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task AddAsync(Room room, CancellationToken cancellationToken = default)
        {
            await _dbContext.Rooms.AddAsync(room, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Room room, CancellationToken cancellationToken = default)
        {
            _dbContext.Rooms.Update(room);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int roomId, CancellationToken cancellationToken = default)
        {
            var room = await _dbContext.Rooms.FindAsync(new object[] { roomId }, cancellationToken: cancellationToken);
            if (room != null)
            {
                _dbContext.Rooms.Remove(room);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}