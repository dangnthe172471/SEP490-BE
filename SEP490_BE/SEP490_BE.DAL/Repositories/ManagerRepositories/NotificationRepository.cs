using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.DTOs.ManagerDTO.Notification;
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
    }
}
