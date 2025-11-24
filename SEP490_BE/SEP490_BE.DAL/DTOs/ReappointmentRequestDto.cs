using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SEP490_BE.DAL.DTOs
{
    #region Request DTOs

    public class CreateReappointmentRequestDto
    {
        [Required(ErrorMessage = "AppointmentId is required")]
        public int AppointmentId { get; set; }

        public DateTime? PreferredDate { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
    }

    public class CompleteReappointmentRequestDto
    {
        [Required(ErrorMessage = "NotificationId is required")]
        public int NotificationId { get; set; }

        [Required(ErrorMessage = "AppointmentDate is required")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "ReasonForVisit is required")]
        [StringLength(500, ErrorMessage = "ReasonForVisit cannot exceed 500 characters")]
        public string ReasonForVisit { get; set; } = string.Empty;
    }

    #endregion

    #region Response DTOs

    public class ReappointmentRequestDto
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }

        // Parsed from Content JSON
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;
        public string? PatientEmail { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSpecialty { get; set; } = string.Empty;
        public DateTime? PreferredDate { get; set; }
        public string? Notes { get; set; }
    }

    // Helper class để parse JSON từ Content
    public class ReappointmentRequestData
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime? PreferredDate { get; set; }
        public string? Notes { get; set; }
    }

    #endregion
}

