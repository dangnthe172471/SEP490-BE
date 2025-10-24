using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IAppointmentRepository
    {
        Task<int?> GetDoctorIdByUserIdAsync(int userId, CancellationToken ct);

        Task<List<Appointment>> GetByDoctorIdAsync(
            int doctorId,
            CancellationToken ct);

        Task<Appointment?> GetDetailForDoctorAsync(
            int doctorId,
            int appointmentId,
            CancellationToken ct);
    }
}
