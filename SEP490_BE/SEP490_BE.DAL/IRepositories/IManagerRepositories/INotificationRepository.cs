using SEP490_BE.DAL.DTOs.ManagerDTO.Notification;
using SEP490_BE.DAL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.IRepositories.IManagerRepository
{
    public interface INotificationRepository
    {
        Task<int> CreateNotificationAsync(CreateNotificationDTO dto);
        Task AddReceiversAsync(int notificationId, List<int> receiverIds);
        Task<List<int>> GetUserIdsByRolesAsync(List<string> roleNames);
        Task<PaginationHelper.PagedResult<NotificationDTO>> GetListNotificationsAsync(int pageNumber, int pageSize);
        Task<PaginationHelper.PagedResult<NotificationDTO>> GetNotificationsByUserAsync(int userId, int pageNumber, int pageSize);
        Task<bool> MarkAsReadAsync(int userId, int notificationId);
        Task<int> CountUnreadAsync(int userId);

        Task MarkAllAsReadAsync(int userId);

        Task UpdateNotificationContentAsync(int notificationId, string content);

    }
}
