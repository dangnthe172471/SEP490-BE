using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IRoomRepository
    {
        Task<List<Room>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Room?> GetByIdAsync(int roomId, CancellationToken cancellationToken = default);
        Task<(List<Room> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);
        Task AddAsync(Room room, CancellationToken cancellationToken = default);
        Task UpdateAsync(Room room, CancellationToken cancellationToken = default);
        Task DeleteAsync(int roomId, CancellationToken cancellationToken = default);
    }
}
