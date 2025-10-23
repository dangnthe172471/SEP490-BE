using SEP490_BE.DAL.DTOs.AppointmentDTO;
using SEP490_BE.DAL.DTOs.Common;

namespace SEP490_BE.BLL.IServices
{
    public interface IAppointmentService
    {
        Task<PagedResult<AppointmentListItemDto>> GetDoctorAppointmentsAsync(
            int userIdFromToken,
            DateTime? from,
            DateTime? to,
            string? status,
            int pageNumber,
            int pageSize,
            CancellationToken ct);

        Task<AppointmentDetailDto?> GetDoctorAppointmentDetailAsync(
            int userIdFromToken,
            int appointmentId,
            CancellationToken ct);
    }
}
