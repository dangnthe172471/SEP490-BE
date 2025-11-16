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

    public bool IsActive { get; set; }

    public string? Avatar { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<DermatologyRecord> DermatologyRecords { get; set; } = new List<DermatologyRecord>();

    public virtual Doctor? Doctor { get; set; }

    public virtual ICollection<NotificationReceiver> NotificationReceivers { get; set; } = new List<NotificationReceiver>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Patient? Patient { get; set; }

    public virtual PharmacyProvider? PharmacyProvider { get; set; }

    public virtual Receptionist? Receptionist { get; set; }

    public virtual Role Role { get; set; } = null!;
}
