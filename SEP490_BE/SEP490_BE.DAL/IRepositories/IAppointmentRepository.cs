using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IAppointmentRepository
    {
        Task<int?> GetDoctorIdByUserIdAsync(int userId, CancellationToken ct);

        Task<PagedResult<Appointment>> GetByDoctorIdAsync(
            int doctorId,
            DateTime? from,
            DateTime? to,
            string? status,
            int pageNumber,
            int pageSize,
            CancellationToken ct);

        Task<Appointment?> GetDetailForDoctorAsync(
            int doctorId,
            int appointmentId,
            CancellationToken ct);
    }
}
