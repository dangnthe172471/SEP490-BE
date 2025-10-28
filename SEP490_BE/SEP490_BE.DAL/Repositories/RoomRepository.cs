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
            // Check if room name already exists
            var existingRoom = await _dbContext.Rooms
                .FirstOrDefaultAsync(r => r.RoomName.ToLower() == room.RoomName.ToLower(), cancellationToken);

            if (existingRoom != null)
            {
                throw new InvalidOperationException($"Room name '{room.RoomName}' already exists.");
            }

            await _dbContext.Rooms.AddAsync(room, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Room room, CancellationToken cancellationToken = default)
        {
            // Check if room name already exists (excluding current room)
            var existingRoom = await _dbContext.Rooms
                .FirstOrDefaultAsync(r => r.RoomName.ToLower() == room.RoomName.ToLower() && r.RoomId != room.RoomId, cancellationToken);

            if (existingRoom != null)
            {
                throw new InvalidOperationException($"Room name '{room.RoomName}' already exists.");
            }

            _dbContext.Rooms.Update(room);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int roomId, CancellationToken cancellationToken = default)
        {
            var room = await _dbContext.Rooms
                .Include(r => r.Doctors)
                .FirstOrDefaultAsync(r => r.RoomId == roomId, cancellationToken);

            if (room != null)
            {
                // Check if room has doctors
                if (room.Doctors.Any())
                {
                    throw new InvalidOperationException($"Cannot delete room '{room.RoomName}' because it has {room.Doctors.Count} doctor(s) assigned.");
                }

                _dbContext.Rooms.Remove(room);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}