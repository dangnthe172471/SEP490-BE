using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class DoctorShift
{
    public int DoctorShiftId { get; set; }

    public int DoctorId { get; set; }

    public int ShiftId { get; set; }

    public string? Status { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual Shift Shift { get; set; } = null!;
}
