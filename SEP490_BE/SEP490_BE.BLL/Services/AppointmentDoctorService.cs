using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.AppointmentDTO;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.IRepositories;

namespace SEP490_BE.BLL.Services
{
    public class AppointmentDoctorService : IAppointmentDoctorService
    {
        private readonly IAppointmentDoctorRepository _repo;

        public AppointmentDoctorService(IAppointmentDoctorRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<AppointmentListItemDto>> GetDoctorAppointmentsAsync(
            int userIdFromToken,
            CancellationToken ct)
        {
            var doctorId = await _repo.GetDoctorIdByUserIdAsync(userIdFromToken, ct);
            if (doctorId is null)
                throw new InvalidOperationException("User hiện tại không phải là bác sĩ.");

            var list = await _repo.GetByDoctorIdAsync(doctorId.Value, ct);

            return list.Select(a => new AppointmentListItemDto
            {
                AppointmentId = a.AppointmentId,
                AppointmentDate = a.AppointmentDate.ToString("dd/MM/yyyy"),
                AppointmentTime = a.AppointmentDate.ToString("HH:mm"),
                Status = a.Status,
                PatientId = a.PatientId,
                PatientName = a.Patient.User.FullName,
                PatientPhone = a.Patient.User.Phone,
                ReasonForVisit = a.ReasonForVisit,
            }).ToList();
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
                AppointmentDate = a.AppointmentDate.ToString("dd/MM/yyyy"),
                AppointmentTime = a.AppointmentDate.ToString("HH:mm"),
                Status = a.Status,
                CreatedAt = a.CreatedAt?.ToString("dd/MM/yyyy HH:mm"),
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor.User.FullName,
                DoctorSpecialty = a.Doctor.Specialty,
                PatientId = a.PatientId,
                PatientName = a.Patient.User.FullName,
                PatientPhone = a.Patient.User.Phone,
                VisitReason = a.ReasonForVisit
            };
        }

    }
}
