using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.DTOs.ManagerDTO.Notification;
using SEP490_BE.DAL.Helpers;
using SEP490_BE.DAL.IRepositories.IManagerRepository;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.Repositories.ManagerRepository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly DiamondHealthContext _context;
        public NotificationRepository(DiamondHealthContext context)
        {
            _context = context;
        }

        public async Task<int> CreateNotificationAsync(CreateNotificationDTO dto)
        {
            var notification = new Notification
            {
                Title = dto.Title,
                Content = dto.Content,
                Type = dto.Type,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.Now,
                IsGlobal = dto.IsGlobal
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification.NotificationId;
        }

        public async Task AddReceiversAsync(int notificationId, List<int> receiverIds)
        {
            var receivers = receiverIds.Select(id => new NotificationReceiver
            {
                NotificationId = notificationId,
                ReceiverId = id,
                IsRead = false
            }).ToList();

            _context.NotificationReceivers.AddRange(receivers);
            await _context.SaveChangesAsync();
        }

        public async Task<List<int>> GetUserIdsByRolesAsync(List<string> roleNames)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => roleNames.Contains(u.Role.RoleName))
                .Select(u => u.UserId)
                .ToListAsync();
        }
        public async Task<PaginationHelper.PagedResult<NotificationDTO>> GetListNotificationsAsync( int pageNumber, int pageSize)
        {
            var query = _context.Notifications
                .Select(n => new NotificationDTO
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Content = n.Content,
                    Type = n.Type,
                    CreatedDate = n.CreatedDate,
                    // Admin không cần cột IsRead, mặc định false
                    IsRead = false
                })
                .OrderByDescending(x => x.NotificationId);

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }
        public async Task<PaginationHelper.PagedResult<NotificationDTO>> GetNotificationsByUserAsync(
    int userId, int pageNumber, int pageSize)
        {
            var query = _context.Notifications
                .Join(
                    _context.NotificationReceivers,
                    n => n.NotificationId,
                    nr => nr.NotificationId,
                    (n, nr) => new { n, nr }
                )
                .Where(x => x.nr.ReceiverId == userId)
                
                .OrderBy(x => x.nr.IsRead)
                .ThenByDescending(x => x.n.CreatedDate)
                .Select(x => new NotificationDTO
                {
                    NotificationId = x.n.NotificationId,
                    Title = x.n.Title,
                    Content = x.n.Content,
                    Type = x.n.Type,
                    CreatedDate = x.n.CreatedDate,
                    IsRead = x.nr.IsRead
                });

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<bool> MarkAsReadAsync(int userId, int notificationId)
        {
            var record = await _context.NotificationReceivers
                .FirstOrDefaultAsync(nr => nr.NotificationId == notificationId && nr.ReceiverId == userId);

            if (record == null) return false;

            record.IsRead = true;
            record.ReadDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CountUnreadAsync(int userId)
        {
            return await _context.NotificationReceivers
                .CountAsync(nr => nr.ReceiverId == userId && nr.IsRead == false);
        }
        public async Task MarkAllAsReadAsync(int userId)
        {
            var unreadList = await _context.NotificationReceivers
                .Where(nr => nr.ReceiverId == userId && !nr.IsRead)
                .ToListAsync();

            if (unreadList.Any())
            {
                foreach (var item in unreadList)
                {
                    item.IsRead = true;
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}
