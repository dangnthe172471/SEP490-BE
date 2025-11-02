using SEP490_BE.DAL.DTOs.ManagerDTO.Notification;
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

    }
}
