using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.AppointmentDTO;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.IRepositories;

namespace SEP490_BE.BLL.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repo;

        public AppointmentService(IAppointmentRepository repo)
        {
            _repo = repo;
        }

        public async Task<PagedResult<AppointmentListItemDto>> GetDoctorAppointmentsAsync(
            int userIdFromToken,
            DateTime? from,
            DateTime? to,
            string? status,
            int pageNumber,
            int pageSize,
            CancellationToken ct)
        {
            var doctorId = await _repo.GetDoctorIdByUserIdAsync(userIdFromToken, ct);
            if (doctorId is null)
                throw new InvalidOperationException("User hiện tại không phải là bác sĩ.");

            var paged = await _repo.GetByDoctorIdAsync(
                doctorId.Value, from, to, status, pageNumber, pageSize, ct);

            return new PagedResult<AppointmentListItemDto>
            {
                Items = paged.Items.Select(a => new AppointmentListItemDto
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = DateOnly.FromDateTime(a.AppointmentDate).ToString("yyyy-MM-dd"),
                    Status = a.Status,
                    PatientId = a.PatientId,
                    PatientName = a.Patient.User.FullName,
                    PatientPhone = a.Patient.User.Phone
                }).ToList(),
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount
            };
        }

        public async Task<AppointmentDetailDto?> GetDoctorAppointmentDetailAsync(
            int userIdFromToken,
            int appointmentId,
            CancellationToken ct)
        {
            var doctorId = await _repo.GetDoctorIdByUserIdAsync(userIdFromToken, ct);
            if (doctorId is null)
                throw new InvalidOperationException("User hiện tại không phải là bác sĩ.");

            var a = await _repo.GetDetailForDoctorAsync(doctorId.Value, appointmentId, ct);
            if (a is null) return null;

            return new AppointmentDetailDto
            {
                AppointmentId = a.AppointmentId,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor.User.FullName,
                DoctorSpecialty = a.Doctor.Specialty,
                PatientId = a.PatientId,
                PatientName = a.Patient.User.FullName,
                PatientPhone = a.Patient.User.Phone
            };
        }
    }
}
