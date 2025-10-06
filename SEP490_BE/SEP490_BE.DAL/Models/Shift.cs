using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class Shift
{
    public int ShiftId { get; set; }

    public int RoomId { get; set; }

    public string ShiftType { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public virtual ICollection<DoctorShift> DoctorShifts { get; set; } = new List<DoctorShift>();

    public virtual Room Room { get; set; } = null!;
}
