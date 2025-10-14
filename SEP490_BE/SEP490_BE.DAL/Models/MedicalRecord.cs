using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class MedicalRecord
{
    public int RecordId { get; set; }

    public int AppointmentId { get; set; }

    public string? DoctorNotes { get; set; }

    public string? Diagnosis { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual InternalMedRecord? InternalMedRecord { get; set; }

    public virtual ObstetricRecord? ObstetricRecord { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual PediatricRecord? PediatricRecord { get; set; }

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
}
