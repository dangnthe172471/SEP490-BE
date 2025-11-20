using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class Receptionist
{
    public int ReceptionistId { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual User User { get; set; } = null!;
}
