using SEP490_BE.BLL.IServices;
using SEP490_BE.BLL.IServices.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.DTOs.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.DTOs.ManagerDTO.Notification;
using SEP490_BE.DAL.DTOs.MedicalRecordDTO;
using SEP490_BE.DAL.IRepositories.IManagerRepository;
using SEP490_BE.DAL.IRepositories.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Services.ManageReceptionist.ManageAppointment
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IEmailServiceApp _emailServiceApp;
        private readonly INotificationRepository _notificationRepository;
        private readonly IMedicalRecordService _medicalRecordService;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IEmailServiceApp emailServiceApp,
            INotificationRepository notificationRepository,
            IMedicalRecordService medicalRecordService)
        {
            _appointmentRepository = appointmentRepository;
            _emailServiceApp = emailServiceApp;
            _notificationRepository = notificationRepository;
            _medicalRecordService = medicalRecordService;
        }

        #region Appointment Methods

        public async Task<List<AppointmentDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
            return appointments.Select(MapToDto).ToList();
        }

        public async Task<AppointmentDto?> GetByIdAsync(int appointmentId, CancellationToken cancellationToken = default)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            return appointment != null ? MapToDto(appointment) : null;
        }

        public async Task<List<AppointmentDto>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
        {
            var appointments = await _appointmentRepository.GetByPatientIdAsync(patientId, cancellationToken);
            return appointments.Select(MapToDto).ToList();
        }

        public async Task<List<AppointmentDto>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default)
        {
            var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorId, cancellationToken);
            return appointments.Select(MapToDto).ToList();
        }

        public async Task<List<AppointmentDto>> GetByReceptionistIdAsync(int receptionistId, CancellationToken cancellationToken = default)
        {
            var appointments = await _appointmentRepository.GetByReceptionistIdAsync(receptionistId, cancellationToken);
            return appointments.Select(MapToDto).ToList();
        }

        // ✅ Helper: validate shift + doctor capacity (<=5)
        private async Task<Shift> ValidateDoctorCapacityPerShiftAsync(
            int doctorId,
            DateTime appointmentDate,
            int? excludeAppointmentId,
            CancellationToken cancellationToken)
        {
            var appointmentTime = TimeOnly.FromDateTime(appointmentDate);
            var shift = await _appointmentRepository.GetShiftByTimeAsync(appointmentTime, cancellationToken);

            if (shift == null)
            {
                throw new ArgumentException(
                    "Thời gian đặt lịch không hợp lệ. Vui lòng chọn thời gian trong các ca: Sáng (08:00-12:00), Chiều (13:30-17:30), Tối (18:00-22:00)."
                );
            }

            var count = await _appointmentRepository.CountAppointmentsByDoctorInShiftAsync(
                appointmentDate,
                doctorId,
                shift.ShiftId,
                excludeAppointmentId,
                cancellationToken
            );

            if (count >= 5)
            {
                // message rõ ràng theo ca
                throw new ArgumentException(
                    $"Bác sĩ đã đủ 5 lịch hẹn trong ca {shift.StartTime:HH\\:mm}-{shift.EndTime:HH\\:mm} ngày {appointmentDate:dd/MM/yyyy}. Vui lòng chọn giờ khác hoặc ngày khác."
                );
            }

            return shift;
        }

        // ✅ Patient tự đặt lịch
        public async Task<int> CreateAppointmentByPatientAsync(BookAppointmentRequest request, int userId, CancellationToken cancellationToken = default)
        {
            // Validate appointment date
            if (request.AppointmentDate < DateTime.Now)
                throw new ArgumentException("Appointment date cannot be in the past.");

            // Get Patient by UserId
            var patient = await _appointmentRepository.GetPatientByUserIdAsync(userId, cancellationToken);

            if (patient == null)
            {
                // fallback tìm theo phone nếu userId giống phone
                if (userId.ToString().Length >= 10)
                {
                    var phoneNumber = userId.ToString();
                    var user = await _appointmentRepository.GetUserByPhoneAsync(phoneNumber, cancellationToken);

                    if (user == null && !phoneNumber.StartsWith("0"))
                        user = await _appointmentRepository.GetUserByPhoneAsync($"0{phoneNumber}", cancellationToken);

                    if (user == null && phoneNumber.StartsWith("0"))
                        user = await _appointmentRepository.GetUserByPhoneAsync(phoneNumber.Substring(1), cancellationToken);

                    if (user != null)
                        patient = await _appointmentRepository.GetPatientByUserIdAsync(user.UserId, cancellationToken);
                }

                if (patient == null)
                    throw new ArgumentException($"Patient not found for UserId = {userId}. Please contact administrator for support.");
            }

            // Validate patient has email for confirmation
            if (string.IsNullOrWhiteSpace(patient.User.Email))
                throw new ArgumentException("Patient email is required for appointment confirmation.");

            // Validate doctor exists
            var doctor = await _appointmentRepository.GetDoctorByIdAsync(request.DoctorId, cancellationToken);
            if (doctor == null)
                throw new ArgumentException("Doctor not found.");

            // ✅ Rule 1: 1 patient chỉ được 1 lịch / 1 ngày (không tính Cancelled)
            var hasExistingAppointment = await _appointmentRepository.HasAppointmentOnDateAsync(
                patient.PatientId,
                request.AppointmentDate,
                cancellationToken);

            if (hasExistingAppointment)
                throw new ArgumentException($"Bạn đã có lịch hẹn vào ngày {request.AppointmentDate:dd/MM/yyyy}. Vui lòng chọn ngày khác.");

            // ✅ Rule 2: 1 bệnh nhân tối đa 5 lần với 1 doctor (không tính Cancelled)
            var countPerDoctor = await _appointmentRepository.CountAppointmentsByPatientAndDoctorAsync(
                patient.PatientId,
                request.DoctorId,
                cancellationToken);

            if (countPerDoctor >= 5)
                throw new ArgumentException("Bạn đã đặt tối đa 5 lịch hẹn với bác sĩ này. Vui lòng chọn bác sĩ khác hoặc liên hệ phòng khám để được hỗ trợ.");

            // ✅ NEW RULE: 1 doctor tối đa 5 lịch / 1 shift / 1 ngày
            await ValidateDoctorCapacityPerShiftAsync(request.DoctorId, request.AppointmentDate, excludeAppointmentId: null, cancellationToken);

            // Create appointment
            var appointment = new Appointment
            {
                PatientId = patient.PatientId,
                DoctorId = request.DoctorId,
                AppointmentDate = request.AppointmentDate,
                ReasonForVisit = request.ReasonForVisit?.Trim(),
                Status = "Confirmed",
                CreatedAt = DateTime.Now,
                ReceptionistId = null
            };

            await _appointmentRepository.AddAsync(appointment, cancellationToken);

            // Send confirmation email
            try
            {
                var confirmation = new AppointmentConfirmationDto
                {
                    AppointmentId = appointment.AppointmentId,
                    PatientName = patient.User.FullName,
                    PatientEmail = patient.User.Email,
                    PatientPhone = patient.User.Phone,
                    DoctorName = doctor.User.FullName,
                    DoctorSpecialty = doctor.Specialty,
                    AppointmentDate = appointment.AppointmentDate,
                    ReasonForVisit = appointment.ReasonForVisit,
                    Status = appointment.Status,
                    CreatedAt = appointment.CreatedAt,
                    ReceptionistName = null
                };

                await _emailServiceApp.SendAppointmentConfirmationEmailAsync(confirmation, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to send confirmation email for Appointment #{appointment.AppointmentId}: {ex.Message}");
            }

            return appointment.AppointmentId;
        }

        // ✅ Receptionist hoặc Doctor tạo appointment cho patient
        public async Task<int> CreateAppointmentByReceptionistAsync(CreateAppointmentByReceptionistRequest request, int receptionistId, CancellationToken cancellationToken = default)
        {
            // Validate appointment date
            if (request.AppointmentDate < DateTime.Now)
                throw new ArgumentException("Appointment date cannot be in the past.");

            // Validate patient exists
            var patient = await _appointmentRepository.GetPatientByIdAsync(request.PatientId, cancellationToken);
            if (patient == null)
                throw new ArgumentException("Patient not found.");

            // Validate doctor exists
            var doctor = await _appointmentRepository.GetDoctorByIdAsync(request.DoctorId, cancellationToken);
            if (doctor == null)
                throw new ArgumentException("Doctor not found.");

            // ✅ NEW RULE: 1 doctor tối đa 5 lịch / 1 shift / 1 ngày
            await ValidateDoctorCapacityPerShiftAsync(request.DoctorId, request.AppointmentDate, excludeAppointmentId: null, cancellationToken);

            // Validate receptionist exists (nếu receptionistId > 0)
            int? receptionistIdNullable = receptionistId > 0 ? receptionistId : null;
            string? receptionistName = null;

            if (receptionistIdNullable.HasValue)
            {
                var receptionist = await _appointmentRepository.GetReceptionistByIdAsync(receptionistIdNullable.Value, cancellationToken);
                if (receptionist == null)
                    throw new ArgumentException("Receptionist not found.");
                receptionistName = receptionist.User.FullName;
            }

            // Create appointment
            var appointment = new Appointment
            {
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                AppointmentDate = request.AppointmentDate,
                ReasonForVisit = request.ReasonForVisit?.Trim(),
                Status = "Confirmed",
                CreatedAt = DateTime.Now,
                ReceptionistId = receptionistIdNullable
            };

            await _appointmentRepository.AddAsync(appointment, cancellationToken);

            // Tạo medical record (nếu chưa có)
            try
            {
                var existingRecord = await _medicalRecordService.GetByAppointmentIdAsync(appointment.AppointmentId, cancellationToken);
                if (existingRecord == null)
                {
                    await _medicalRecordService.CreateAsync(new CreateMedicalRecordDto
                    {
                        AppointmentId = appointment.AppointmentId,
                        DoctorNotes = null,
                        Diagnosis = null
                    }, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to create medical record: {ex.Message}");
            }

            // Send confirmation email if patient has email
            if (!string.IsNullOrWhiteSpace(patient.User.Email))
            {
                try
                {
                    var confirmation = new AppointmentConfirmationDto
                    {
                        AppointmentId = appointment.AppointmentId,
                        PatientName = patient.User.FullName,
                        PatientEmail = patient.User.Email,
                        PatientPhone = patient.User.Phone,
                        DoctorName = doctor.User.FullName,
                        DoctorSpecialty = doctor.Specialty,
                        AppointmentDate = appointment.AppointmentDate,
                        ReasonForVisit = appointment.ReasonForVisit,
                        Status = appointment.Status,
                        CreatedAt = appointment.CreatedAt,
                        ReceptionistName = receptionistName
                    };

                    await _emailServiceApp.SendAppointmentConfirmationEmailAsync(confirmation, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to send confirmation email: {ex.Message}");
                }
            }

            // Notification cho doctor nếu Doctor tự tạo (ReceptionistId = null)
            if (!receptionistIdNullable.HasValue && doctor.UserId > 0)
            {
                try
                {
                    var notificationContent = new
                    {
                        AppointmentId = appointment.AppointmentId,
                        PatientId = appointment.PatientId,
                        PatientName = patient.User.FullName,
                        DoctorId = appointment.DoctorId,
                        DoctorName = doctor.User.FullName,
                        AppointmentDate = appointment.AppointmentDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                        ReasonForVisit = appointment.ReasonForVisit
                    };

                    var notificationDto = new CreateNotificationDTO
                    {
                        Title = $"Lịch tái khám đã được đặt cho {patient.User.FullName}",
                        Content = JsonSerializer.Serialize(notificationContent),
                        Type = "Reappointment",
                        CreatedBy = doctor.UserId,
                        IsGlobal = false,
                        ReceiverIds = new List<int> { doctor.UserId }
                    };

                    var notificationId = await _notificationRepository.CreateNotificationAsync(notificationDto);
                    await _notificationRepository.AddReceiversAsync(notificationId, new List<int> { doctor.UserId });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to create notification for doctor: {ex.Message}");
                }
            }

            return appointment.AppointmentId;
        }

        public async Task<bool> RescheduleAppointmentAsync(int appointmentId, int userId, RescheduleAppointmentRequest request, CancellationToken cancellationToken = default)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null)
                return false;

            var patient = await _appointmentRepository.GetPatientByUserIdAsync(userId, cancellationToken);
            if (patient == null)
                throw new UnauthorizedAccessException("Patient not found.");

            if (appointment.PatientId != patient.PatientId)
                throw new UnauthorizedAccessException("You can only reschedule your own appointments.");

            if (appointment.Status == "Completed" || appointment.Status == "Cancelled")
                throw new InvalidOperationException("Cannot reschedule completed or cancelled appointments.");

            if (request.NewAppointmentDate < DateTime.Now)
                throw new ArgumentException("New appointment date cannot be in the past.");

            // ✅ NEW RULE: 1 doctor tối đa 5 lịch / 1 shift / 1 ngày (exclude chính appointment đang reschedule)
            await ValidateDoctorCapacityPerShiftAsync(
                appointment.DoctorId,
                request.NewAppointmentDate,
                excludeAppointmentId: appointment.AppointmentId,
                cancellationToken
            );

            appointment.AppointmentDate = request.NewAppointmentDate;
            if (!string.IsNullOrWhiteSpace(request.NewReasonForVisit))
                appointment.ReasonForVisit = request.NewReasonForVisit.Trim();

            appointment.UpdatedBy = userId;

            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);

            try
            {
                var confirmation = new AppointmentConfirmationDto
                {
                    AppointmentId = appointment.AppointmentId,
                    PatientName = appointment.Patient.User.FullName,
                    PatientEmail = appointment.Patient.User.Email ?? string.Empty,
                    PatientPhone = appointment.Patient.User.Phone,
                    DoctorName = appointment.Doctor.User.FullName,
                    DoctorSpecialty = appointment.Doctor.Specialty,
                    AppointmentDate = appointment.AppointmentDate,
                    ReasonForVisit = appointment.ReasonForVisit,
                    Status = appointment.Status,
                    CreatedAt = appointment.CreatedAt,
                    ReceptionistName = appointment.Receptionist?.User?.FullName
                };

                await _emailServiceApp.SendAppointmentRescheduleEmailAsync(confirmation, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to send reschedule email: {ex.Message}");
            }

            return true;
        }

        public async Task<bool> UpdateAppointmentStatusAsync(int appointmentId, UpdateAppointmentStatusRequest request, CancellationToken cancellationToken = default)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null) return false;

            if (string.IsNullOrWhiteSpace(request.Status))
                throw new ArgumentException("Status is required.");

            var validStatuses = new[] { "Pending", "Confirmed", "Completed", "Cancelled", "No-Show" };
            if (!validStatuses.Contains(request.Status))
                throw new ArgumentException($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");

            if (request.Status == "Cancelled")
            {
                var timeDifference = appointment.AppointmentDate - DateTime.Now;
                if (timeDifference.TotalHours < 4)
                {
                    throw new ArgumentException(
                        $"Không thể hủy lịch hẹn. Bạn chỉ có thể hủy trước tối thiểu 4 giờ so với giờ hẹn. " +
                        $"Thời gian còn lại: {timeDifference.TotalHours:F1} giờ. Vui lòng liên hệ trực tiếp với phòng khám để được hỗ trợ."
                    );
                }
            }

            var oldStatus = appointment.Status;
            appointment.Status = request.Status;

            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);

            try
            {
                var confirmation = new AppointmentConfirmationDto
                {
                    AppointmentId = appointment.AppointmentId,
                    PatientName = appointment.Patient.User.FullName,
                    PatientEmail = appointment.Patient.User.Email ?? string.Empty,
                    PatientPhone = appointment.Patient.User.Phone,
                    DoctorName = appointment.Doctor.User.FullName,
                    DoctorSpecialty = appointment.Doctor.Specialty,
                    AppointmentDate = appointment.AppointmentDate,
                    ReasonForVisit = appointment.ReasonForVisit,
                    Status = appointment.Status,
                    CreatedAt = appointment.CreatedAt,
                    ReceptionistName = appointment.Receptionist?.User?.FullName
                };

                if (request.Status == "Cancelled")
                    await _emailServiceApp.SendAppointmentCancellationEmailAsync(confirmation, cancellationToken);
                else if (oldStatus != request.Status)
                    await _emailServiceApp.SendAppointmentStatusUpdateEmailAsync(confirmation, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to send status update email: {ex.Message}");
            }

            return true;
        }

        public async Task<bool> CanCancelAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null) return false;

            var cancellableStatuses = new[] { "Pending", "Confirmed" };
            if (!cancellableStatuses.Contains(appointment.Status)) return false;

            var timeDifference = appointment.AppointmentDate - DateTime.Now;
            return timeDifference.TotalHours >= 4;
        }

        public async Task<AppointmentConfirmationDto?> GetAppointmentConfirmationAsync(int appointmentId, CancellationToken cancellationToken = default)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null) return null;

            return new AppointmentConfirmationDto
            {
                AppointmentId = appointment.AppointmentId,
                PatientName = appointment.Patient.User.FullName,
                PatientEmail = appointment.Patient.User.Email ?? string.Empty,
                PatientPhone = appointment.Patient.User.Phone,
                DoctorName = appointment.Doctor.User.FullName,
                DoctorSpecialty = appointment.Doctor.Specialty,
                AppointmentDate = appointment.AppointmentDate,
                ReasonForVisit = appointment.ReasonForVisit,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt,
                ReceptionistName = appointment.Receptionist?.User?.FullName
            };
        }

        public async Task<AppointmentStatisticsDto> GetAppointmentStatisticsAsync(CancellationToken cancellationToken = default)
        {
            var stats = await _appointmentRepository.GetAppointmentStatisticsAsync(cancellationToken);

            return new AppointmentStatisticsDto
            {
                TotalAppointments = stats.GetValueOrDefault("Total", 0),
                PendingAppointments = stats.GetValueOrDefault("Pending", 0),
                ConfirmedAppointments = stats.GetValueOrDefault("Confirmed", 0),
                CompletedAppointments = stats.GetValueOrDefault("Completed", 0),
                CancelledAppointments = stats.GetValueOrDefault("Cancelled", 0),
                NoShowAppointments = stats.GetValueOrDefault("No-Show", 0)
            };
        }

        public Task<List<AppointmentTimeSeriesPointDto>> GetAppointmentTimeSeriesAsync(DateTime? from, DateTime? to, string groupBy, CancellationToken cancellationToken = default)
            => _appointmentRepository.GetAppointmentTimeSeriesAsync(from, to, groupBy, cancellationToken);

        public Task<List<AppointmentHeatmapPointDto>> GetAppointmentHeatmapAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
            => _appointmentRepository.GetAppointmentHeatmapAsync(from, to, cancellationToken);

        public async Task<bool> DeleteAsync(int appointmentId, CancellationToken cancellationToken = default)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null) return false;

            await _appointmentRepository.DeleteAsync(appointmentId, cancellationToken);
            return true;
        }

        #endregion

        #region Doctor Methods

        public async Task<List<DoctorInfoDto>> GetAllDoctorsAsync(CancellationToken cancellationToken = default)
        {
            var doctors = await _appointmentRepository.GetAllDoctorsAsync(cancellationToken);
            return doctors.Select(MapToDoctorDto).ToList();
        }

        public async Task<DoctorInfoDto?> GetDoctorByIdAsync(int doctorId, CancellationToken cancellationToken = default)
        {
            var doctor = await _appointmentRepository.GetDoctorByIdAsync(doctorId, cancellationToken);
            return doctor != null ? MapToDoctorDto(doctor) : null;
        }

        public async Task<DoctorInfoDto?> GetDoctorByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            var doctor = await _appointmentRepository.GetDoctorByUserIdAsync(userId, cancellationToken);
            return doctor != null ? MapToDoctorDto(doctor) : null;
        }

        #endregion

        #region Patient Methods

        public async Task<PatientInfoDto?> GetPatientByIdAsync(int patientId, CancellationToken cancellationToken = default)
        {
            var patient = await _appointmentRepository.GetPatientByIdAsync(patientId, cancellationToken);
            return patient != null ? MapToPatientDto(patient) : null;
        }

        public async Task<PatientInfoDto?> GetPatientInfoByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            var patient = await _appointmentRepository.GetPatientByUserIdAsync(userId, cancellationToken);
            return patient != null ? MapToPatientDto(patient) : null;
        }

        #endregion

        #region Receptionist Methods

        public async Task<ReceptionistInfoDto?> GetReceptionistByIdAsync(int receptionistId, CancellationToken cancellationToken = default)
        {
            var receptionist = await _appointmentRepository.GetReceptionistByIdAsync(receptionistId, cancellationToken);
            return receptionist != null ? MapToReceptionistDto(receptionist) : null;
        }

        public async Task<ReceptionistInfoDto?> GetReceptionistByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            var receptionist = await _appointmentRepository.GetReceptionistByUserIdAsync(userId, cancellationToken);
            return receptionist != null ? MapToReceptionistDto(receptionist) : null;
        }

        #endregion

        #region Debug Methods

        public async Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
            => await _appointmentRepository.GetUserByIdAsync(userId, cancellationToken);

        public async Task<Patient?> GetPatientEntityByUserIdAsync(int userId, CancellationToken cancellationToken = default)
            => await _appointmentRepository.GetPatientByUserIdAsync(userId, cancellationToken);

        #endregion

        #region Helper Methods

        private AppointmentDto MapToDto(Appointment appointment)
        {
            return new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                PatientId = appointment.PatientId,
                PatientName = appointment.Patient?.User?.FullName ?? "Unknown",
                PatientPhone = appointment.Patient?.User?.Phone ?? "Unknown",
                PatientEmail = appointment.Patient?.User?.Email,
                DoctorId = appointment.DoctorId,
                DoctorName = appointment.Doctor?.User?.FullName ?? "Unknown",
                DoctorSpecialty = appointment.Doctor?.Specialty ?? "Unknown",
                AppointmentDate = appointment.AppointmentDate,
                ReceptionistId = appointment.ReceptionistId,
                ReceptionistName = appointment.Receptionist?.User?.FullName,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt,
                ReasonForVisit = appointment.ReasonForVisit,
                UpdatedBy = appointment.UpdatedBy
            };
        }

        private DoctorInfoDto MapToDoctorDto(Doctor doctor)
        {
            return new DoctorInfoDto
            {
                DoctorId = doctor.DoctorId,
                UserId = doctor.UserId,
                FullName = doctor.User.FullName,
                Email = doctor.User.Email ?? string.Empty,
                Phone = doctor.User.Phone,
                Specialty = doctor.Specialty,
                ExperienceYears = doctor.ExperienceYears,
                RoomId = doctor.RoomId,
                RoomName = doctor.Room?.RoomName ?? string.Empty
            };
        }

        private PatientInfoDto MapToPatientDto(Patient patient)
        {
            return new PatientInfoDto
            {
                PatientId = patient.PatientId,
                UserId = patient.UserId,
                FullName = patient.User.FullName,
                Email = patient.User.Email ?? string.Empty,
                Phone = patient.User.Phone,
                Allergies = patient.Allergies,
                MedicalHistory = patient.MedicalHistory
            };
        }

        private ReceptionistInfoDto MapToReceptionistDto(Receptionist receptionist)
        {
            return new ReceptionistInfoDto
            {
                ReceptionistId = receptionist.ReceptionistId,
                UserId = receptionist.UserId,
                FullName = receptionist.User.FullName,
                Email = receptionist.User.Email ?? string.Empty,
                Phone = receptionist.User.Phone
            };
        }

        #endregion
    }
}
