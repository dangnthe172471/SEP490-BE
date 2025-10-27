using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;

        public RoomService(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<IEnumerable<RoomDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var rooms = await _roomRepository.GetAllAsync(cancellationToken);
            return rooms.Select(r => new RoomDto
            {
                RoomId = r.RoomId,
                RoomName = r.RoomName
            });
        }

        public async Task<RoomDto?> GetByIdAsync(int roomId, CancellationToken cancellationToken = default)
        {
            var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken);
            if (room == null)
            {
                return null;
            }

            return new RoomDto
            {
                RoomId = room.RoomId,
                RoomName = room.RoomName
            };
        }

        public async Task<PagedResponse<RoomDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var (items, totalCount) = await _roomRepository.GetPagedAsync(pageNumber, pageSize, searchTerm, cancellationToken);

            return new PagedResponse<RoomDto>
            {
                Items = items.Select(r => new RoomDto
                {
                    RoomId = r.RoomId,
                    RoomName = r.RoomName
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<int> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.RoomName))
            {
                throw new ArgumentException("Room name is required.");
            }

            var trimmedName = request.RoomName.Trim();

            // Validate room name length
            if (trimmedName.Length < 2)
            {
                throw new ArgumentException("Room name must be at least 2 characters long.");
            }

            if (trimmedName.Length > 100)
            {
                throw new ArgumentException("Room name cannot exceed 100 characters.");
            }

            // Validate room name format (only letters, numbers, spaces, and common punctuation)
            if (!System.Text.RegularExpressions.Regex.IsMatch(trimmedName, @"^[\p{L}\p{N}\s\-_.,()@#$%^&*]+$"))
            {
                throw new ArgumentException("Room name contains invalid characters. Only letters, numbers, spaces, and common punctuation are allowed.");
            }

            var room = new Room
            {
                RoomName = trimmedName
            };

            await _roomRepository.AddAsync(room, cancellationToken);
            return room.RoomId;
        }

        public async Task<RoomDto?> UpdateAsync(int roomId, UpdateRoomRequest request, CancellationToken cancellationToken = default)
        {
            var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken);
            if (room == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(request.RoomName))
            {
                throw new ArgumentException("Room name is required.");
            }

            var trimmedName = request.RoomName.Trim();

            // Validate room name length
            if (trimmedName.Length < 2)
            {
                throw new ArgumentException("Room name must be at least 2 characters long.");
            }

            if (trimmedName.Length > 100)
            {
                throw new ArgumentException("Room name cannot exceed 100 characters.");
            }

            // Validate room name format (only letters, numbers, spaces, and common punctuation)
            if (!System.Text.RegularExpressions.Regex.IsMatch(trimmedName, @"^[\p{L}\p{N}\s\-_.,()@#$%^&*]+$"))
            {
                throw new ArgumentException("Room name contains invalid characters. Only letters, numbers, spaces, and common punctuation are allowed.");
            }

            room.RoomName = trimmedName;

            await _roomRepository.UpdateAsync(room, cancellationToken);

            return new RoomDto
            {
                RoomId = room.RoomId,
                RoomName = room.RoomName
            };
        }

        public async Task<bool> DeleteAsync(int roomId, CancellationToken cancellationToken = default)
        {
            var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken);
            if (room == null)
            {
                return false;
            }

            await _roomRepository.DeleteAsync(roomId, cancellationToken);
            return true;
        }
    }
}