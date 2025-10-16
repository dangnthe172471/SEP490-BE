using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class DoctorShift
{
    public int DoctorShiftId { get; set; }

    public int DoctorId { get; set; }

    public int ShiftId { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public string? Status { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual ICollection<DoctorShiftExchange> DoctorShiftExchangeDoctor1ShiftRefs { get; set; } = new List<DoctorShiftExchange>();

    public virtual ICollection<DoctorShiftExchange> DoctorShiftExchangeDoctor2ShiftRefs { get; set; } = new List<DoctorShiftExchange>();

    public virtual Shift Shift { get; set; } = null!;
}
