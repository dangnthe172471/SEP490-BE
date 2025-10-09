using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class Doctor
{
    public int DoctorId { get; set; }

    public int UserId { get; set; }

    public string Specialty { get; set; } = null!;

    public int ExperienceYears { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<DoctorShift> DoctorShifts { get; set; } = new List<DoctorShift>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual User User { get; set; } = null!;
}
