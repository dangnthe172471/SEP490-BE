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

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
