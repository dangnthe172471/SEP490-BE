using SEP490_BE.DAL.DTOs.ManagerDTO.Notification;
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

    }
}
