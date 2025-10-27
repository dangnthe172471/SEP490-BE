using SEP490_BE.BLL.IServices.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.DTOs.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.IRepositories.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Services.ManageReceptionist.ManageAppointment
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IEmailServiceApp _emailServiceApp;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IEmailServiceApp emailServiceApp)
        {
            _appointmentRepository = appointmentRepository;
            _emailServiceApp = emailServiceApp;
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

        public async Task<int> CreateAppointmentByPatientAsync(BookAppointmentRequest request, int userId, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"[DEBUG] === CREATE APPOINTMENT BY PATIENT ===");
            Console.WriteLine($"[DEBUG] UserId from token: {userId}");

            // Validate appointment date
            if (request.AppointmentDate < DateTime.Now)
            {
                throw new ArgumentException("Appointment date cannot be in the past.");
            }

            // Get Patient by UserId
            var patient = await _appointmentRepository.GetPatientByUserIdAsync(userId, cancellationToken);

            if (patient == null)
            {
                Console.WriteLine($"[WARNING] No Patient found with UserId = {userId}. Trying to find by phone number...");

                // Try to find User by phone if userId looks like phone number
                if (userId.ToString().Length >= 10)
                {
                    Console.WriteLine($"[DEBUG] UserId looks like phone number: {userId}");

                    // Try different phone formats
                    var phoneNumber = userId.ToString();
                    var user = await _appointmentRepository.GetUserByPhoneAsync(phoneNumber, cancellationToken);

                    // Try with leading zero
                    if (user == null && !phoneNumber.StartsWith("0"))
                    {
                        Console.WriteLine($"[DEBUG] Trying with leading zero: 0{phoneNumber}");
                        user = await _appointmentRepository.GetUserByPhoneAsync($"0{phoneNumber}", cancellationToken);
                    }

                    // Try without leading zero
                    if (user == null && phoneNumber.StartsWith("0"))
                    {
                        Console.WriteLine($"[DEBUG] Trying without leading zero: {phoneNumber.Substring(1)}");
                        user = await _appointmentRepository.GetUserByPhoneAsync(phoneNumber.Substring(1), cancellationToken);
                    }

                    if (user != null)
                    {
                        Console.WriteLine($"[DEBUG] Found User by phone: UserId = {user.UserId}, Phone = {user.Phone}");
                        patient = await _appointmentRepository.GetPatientByUserIdAsync(user.UserId, cancellationToken);
                        if (patient != null)
                        {
                            Console.WriteLine($"[SUCCESS] Found Patient by UserId: PatientId = {patient.PatientId}, UserId = {patient.UserId}");
                        }
                    }
                }

                if (patient == null)
                {
                    throw new ArgumentException($"Patient not found for UserId = {userId}. Please contact administrator for support.");
                }
            }

            Console.WriteLine($"[DEBUG] Found Patient: PatientId = {patient.PatientId}, UserId = {patient.UserId}, Name = {patient.User.FullName}");

            // Validate patient has email for confirmation
            if (string.IsNullOrWhiteSpace(patient.User.Email))
            {
                throw new ArgumentException("Patient email is required for appointment confirmation.");
            }

            // Validate doctor exists
            var doctor = await _appointmentRepository.GetDoctorByIdAsync(request.DoctorId, cancellationToken);
            if (doctor == null)
            {
                throw new ArgumentException("Doctor not found.");
            }

            // Create appointment
            var appointment = new Appointment
            {
                PatientId = patient.PatientId,
                DoctorId = request.DoctorId,
                AppointmentDate = request.AppointmentDate,
                ReasonForVisit = request.ReasonForVisit?.Trim(),
                Status = "Confirmed", // Patient tự đặt thì confirmed luôn
                CreatedAt = DateTime.Now,
                ReceptionistId = null // Patient tự đặt nên không có ReceptionistId
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
                    ReceptionistName = null // Patient tự đặt
                };

                await _emailServiceApp.SendAppointmentConfirmationEmailAsync(confirmation, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to send confirmation email for Appointment #{appointment.AppointmentId}: {ex.Message}");
            }

            return appointment.AppointmentId;
        }

        public async Task<int> CreateAppointmentByReceptionistAsync(CreateAppointmentByReceptionistRequest request, int receptionistId, CancellationToken cancellationToken = default)
        {
            // Validate appointment date
            if (request.AppointmentDate < DateTime.Now)
            {
                throw new ArgumentException("Appointment date cannot be in the past.");
            }

            // Validate patient exists
            var patient = await _appointmentRepository.GetPatientByIdAsync(request.PatientId, cancellationToken);
            if (patient == null)
            {
                throw new ArgumentException("Patient not found.");
            }

            // Validate doctor exists
            var doctor = await _appointmentRepository.GetDoctorByIdAsync(request.DoctorId, cancellationToken);
            if (doctor == null)
            {
                throw new ArgumentException("Doctor not found.");
            }

            // Validate receptionist exists
            var receptionist = await _appointmentRepository.GetReceptionistByIdAsync(receptionistId, cancellationToken);
            if (receptionist == null)
            {
                throw new ArgumentException("Receptionist not found.");
            }

            // Create appointment
            var appointment = new Appointment
            {
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                AppointmentDate = request.AppointmentDate,
                ReasonForVisit = request.ReasonForVisit?.Trim(),
                Status = "Confirmed", // Receptionist tạo thì pending
                CreatedAt = DateTime.Now,
                ReceptionistId = receptionistId // Có ReceptionistId
            };

            await _appointmentRepository.AddAsync(appointment, cancellationToken);

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
                        ReceptionistName = receptionist.User.FullName
                    };

                    await _emailServiceApp.SendAppointmentConfirmationEmailAsync(confirmation, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to send confirmation email: {ex.Message}");
                }
            }

            return appointment.AppointmentId;
        }

        public async Task<bool> RescheduleAppointmentAsync(int appointmentId, int userId, RescheduleAppointmentRequest request, CancellationToken cancellationToken = default)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null)
            {
                return false;
            }

            // Get Patient by UserId to verify ownership
            var patient = await _appointmentRepository.GetPatientByUserIdAsync(userId, cancellationToken);
            if (patient == null)
            {
                throw new UnauthorizedAccessException("Patient not found.");
            }

            // Verify patient owns this appointment
            if (appointment.PatientId != patient.PatientId)
            {
                throw new UnauthorizedAccessException("You can only reschedule your own appointments.");
            }

            // Cannot reschedule completed or cancelled appointments
            if (appointment.Status == "Completed" || appointment.Status == "Cancelled")
            {
                throw new InvalidOperationException("Cannot reschedule completed or cancelled appointments.");
            }

            if (request.NewAppointmentDate < DateTime.Now)
            {
                throw new ArgumentException("New appointment date cannot be in the past.");
            }

            appointment.AppointmentDate = request.NewAppointmentDate;
            if (!string.IsNullOrWhiteSpace(request.NewReasonForVisit))
            {
                appointment.ReasonForVisit = request.NewReasonForVisit.Trim();
            }
            appointment.UpdatedBy = userId;

            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);

            // Send reschedule email
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
            if (appointment == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.Status))
            {
                throw new ArgumentException("Status is required.");
            }

            var validStatuses = new[] { "Pending", "Confirmed", "Completed", "Cancelled", "No-Show" };
            if (!validStatuses.Contains(request.Status))
            {
                throw new ArgumentException($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");
            }

            // ✅ Kiểm tra quy tắc hủy lịch: Phải hủy trước tối thiểu 4 giờ
            if (request.Status == "Cancelled")
            {
                var currentTime = DateTime.Now;
                var appointmentTime = appointment.AppointmentDate;
                var timeDifference = appointmentTime - currentTime;

                Console.WriteLine($"[DEBUG] Cancel Appointment Check:");
                Console.WriteLine($"[DEBUG] Current Time: {currentTime:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"[DEBUG] Appointment Time: {appointmentTime:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"[DEBUG] Time Difference: {timeDifference.TotalHours:F2} hours");

                // Kiểm tra nếu còn ít hơn 4 giờ
                if (timeDifference.TotalHours < 4)
                {
                    throw new ArgumentException($"Không thể hủy lịch hẹn. Bạn chỉ có thể hủy trước tối thiểu 4 giờ so với giờ hẹn. Thời gian còn lại: {timeDifference.TotalHours:F1} giờ. Vui lòng liên hệ trực tiếp với phòng khám để được hỗ trợ.");
                }

                Console.WriteLine($"[DEBUG] Cancel allowed - Time difference: {timeDifference.TotalHours:F2} hours (>= 4 hours)");
            }

            var oldStatus = appointment.Status;
            appointment.Status = request.Status;
            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);

            // Send status update email
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
                {
                    await _emailServiceApp.SendAppointmentCancellationEmailAsync(confirmation, cancellationToken);
                }
                else if (oldStatus != request.Status)
                {
                    await _emailServiceApp.SendAppointmentStatusUpdateEmailAsync(confirmation, cancellationToken);
                }
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
            if (appointment == null)
            {
                return false;
            }

            // Kiểm tra status có thể hủy không
            var cancellableStatuses = new[] { "Pending", "Confirmed" };
            if (!cancellableStatuses.Contains(appointment.Status))
            {
                return false;
            }

            // Kiểm tra quy tắc 4 giờ
            var currentTime = DateTime.Now;
            var appointmentTime = appointment.AppointmentDate;
            var timeDifference = appointmentTime - currentTime;

            Console.WriteLine($"[DEBUG] CanCancelAppointment Check:");
            Console.WriteLine($"[DEBUG] Appointment ID: {appointmentId}");
            Console.WriteLine($"[DEBUG] Current Status: {appointment.Status}");
            Console.WriteLine($"[DEBUG] Current Time: {currentTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"[DEBUG] Appointment Time: {appointmentTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"[DEBUG] Time Difference: {timeDifference.TotalHours:F2} hours");

            var canCancel = timeDifference.TotalHours >= 4;
            Console.WriteLine($"[DEBUG] Can Cancel: {canCancel} (>= 4 hours: {timeDifference.TotalHours >= 4})");

            return canCancel;
        }

        public async Task<AppointmentConfirmationDto?> GetAppointmentConfirmationAsync(int appointmentId, CancellationToken cancellationToken = default)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null)
            {
                return null;
            }

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

        public async Task<bool> DeleteAsync(int appointmentId, CancellationToken cancellationToken = default)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null)
            {
                return false;
            }

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
        {
            return await _appointmentRepository.GetUserByIdAsync(userId, cancellationToken);
        }

        public async Task<Patient?> GetPatientEntityByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _appointmentRepository.GetPatientByUserIdAsync(userId, cancellationToken);
        }

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