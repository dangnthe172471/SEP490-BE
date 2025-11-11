using System.ComponentModel.DataAnnotations;

namespace SEP490_BE.DAL.DTOs.ManageReceptionist.ManageAppointment
{
    #region Request DTOs

    public class BookAppointmentRequest
    {
        [Required(ErrorMessage = "DoctorId is required")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "AppointmentDate is required")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "ReasonForVisit is required")]
        [StringLength(500, ErrorMessage = "ReasonForVisit cannot exceed 500 characters")]
        public string ReasonForVisit { get; set; } = string.Empty;
    }

    public class CreateAppointmentByReceptionistRequest
    {
        [Required(ErrorMessage = "PatientId is required")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "DoctorId is required")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "AppointmentDate is required")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "ReasonForVisit is required")]
        [StringLength(500, ErrorMessage = "ReasonForVisit cannot exceed 500 characters")]
        public string ReasonForVisit { get; set; } = string.Empty;
    }

    public class RescheduleAppointmentRequest
    {
        [Required(ErrorMessage = "NewAppointmentDate is required")]
        public DateTime NewAppointmentDate { get; set; }

        [StringLength(500, ErrorMessage = "NewReasonForVisit cannot exceed 500 characters")]
        public string? NewReasonForVisit { get; set; }
    }

    public class UpdateAppointmentStatusRequest
    {
        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = string.Empty;
    }

    #endregion

    #region Response DTOs

    public class AppointmentDto
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;
        public string? PatientEmail { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSpecialty { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public int? ReceptionistId { get; set; }
        public string? ReceptionistName { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? ReasonForVisit { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class AppointmentConfirmationDto
    {
        public int AppointmentId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientEmail { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSpecialty { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string? ReasonForVisit { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? ReceptionistName { get; set; }
    }

    public class AppointmentStatisticsDto
    {
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int NoShowAppointments { get; set; }
    }

    // --- New statistics DTOs ---
    public class AppointmentTimeSeriesPointDto
    {
        public string Period { get; set; } = string.Empty; // yyyy-MM-dd | yyyy-MM
        public int Count { get; set; }
    }

    public class AppointmentHeatmapPointDto
    {
        public int Weekday { get; set; } // 0=Sunday..6=Saturday
        public int Hour { get; set; }    // 0..23
        public int Count { get; set; }
    }

    public class DoctorInfoDto
    {
        public int DoctorId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
    }

    public class PatientInfoDto
    {
        public int PatientId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Allergies { get; set; }
        public string? MedicalHistory { get; set; }
    }

    public class ReceptionistInfoDto
    {
        public int ReceptionistId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    #endregion
}