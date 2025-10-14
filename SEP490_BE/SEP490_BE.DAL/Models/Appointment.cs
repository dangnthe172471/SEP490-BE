using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public int? ReceptionistId { get; set; }

    public int? UpdatedBy { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    public virtual Patient Patient { get; set; } = null!;

    public virtual Receptionist? Receptionist { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
