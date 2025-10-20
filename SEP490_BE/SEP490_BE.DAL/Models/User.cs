using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Phone { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public DateOnly? Dob { get; set; }

    public string? Gender { get; set; }

    public int RoleId { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Doctor? Doctor { get; set; }

    public virtual Patient? Patient { get; set; }

    public virtual PharmacyProvider? PharmacyProvider { get; set; }

    public virtual Receptionist? Receptionist { get; set; }

    public virtual Role Role { get; set; } = null!;
}
