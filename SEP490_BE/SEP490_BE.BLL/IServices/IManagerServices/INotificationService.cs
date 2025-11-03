using SEP490_BE.DAL.DTOs.ManagerDTO.Notification;
using SEP490_BE.DAL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.IServices.IManagerService
{
    public interface INotificationService
    {
        Task SendAppointmentReminderAsync(CancellationToken cancellationToken = default);
        Task SendNotificationAsync(CreateNotificationDTO dto);
        Task<PaginationHelper.PagedResult<NotificationDTO>> GetNotificationsByUserAsync(int userId, int pageNumber, int pageSize);
        Task<bool> MarkAsReadAsync(int userId, int notificationId);
        Task<int> CountUnreadAsync(int userId);
        Task MarkAllAsReadAsync(int userId);
    }
}
