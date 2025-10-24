using SEP490_BE.DAL.DTOs.ManageReceptionist.ManageAppointment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.IServices.ManageReceptionist.ManageAppointment
{
    public interface IEmailServiceApp
    {
        Task SendAppointmentConfirmationEmailAsync(AppointmentConfirmationDto confirmation, CancellationToken cancellationToken = default);
        Task SendAppointmentRescheduleEmailAsync(AppointmentConfirmationDto confirmation, CancellationToken cancellationToken = default);
        Task SendAppointmentCancellationEmailAsync(AppointmentConfirmationDto confirmation, CancellationToken cancellationToken = default);
        Task SendAppointmentStatusUpdateEmailAsync(AppointmentConfirmationDto confirmation, CancellationToken cancellationToken = default);
    }
}