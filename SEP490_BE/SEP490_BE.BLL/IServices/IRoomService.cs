using SEP490_BE.DAL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.IServices
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<RoomDto?> GetByIdAsync(int roomId, CancellationToken cancellationToken = default);
        Task<PagedResponse<RoomDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<int> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken = default);
        Task<RoomDto?> UpdateAsync(int roomId, UpdateRoomRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int roomId, CancellationToken cancellationToken = default);
    }
}
