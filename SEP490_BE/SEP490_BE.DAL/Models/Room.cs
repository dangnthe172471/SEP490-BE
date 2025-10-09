using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string RoomName { get; set; } = null!;

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}
