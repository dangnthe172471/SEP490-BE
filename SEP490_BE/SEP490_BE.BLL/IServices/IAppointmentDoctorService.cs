using SEP490_BE.DAL.DTOs.AppointmentDTO;
using SEP490_BE.DAL.DTOs.Common;

namespace SEP490_BE.BLL.IServices
{
    public interface IAppointmentDoctorService
    {
        Task<List<AppointmentListItemDto>> GetDoctorAppointmentsAsync(
            int userIdFromToken,
            CancellationToken ct);

        Task<AppointmentDetailDto?> GetDoctorAppointmentDetailAsync(
            int userIdFromToken,
            int appointmentId,
            CancellationToken ct);
    }
}
