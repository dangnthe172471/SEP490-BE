using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class DoctorShiftExchange
{
    public int ExchangeId { get; set; }

    public int Doctor1Id { get; set; }

    public int Doctor1ShiftRefId { get; set; }

    public int? Doctor2Id { get; set; }

    public int? Doctor2ShiftRefId { get; set; }

    public DateOnly ExchangeDate { get; set; }

    public string? Status { get; set; }

    public virtual Doctor Doctor1 { get; set; } = null!;

    public virtual DoctorShift Doctor1ShiftRef { get; set; } = null!;

    public virtual Doctor? Doctor2 { get; set; }

    public virtual DoctorShift? Doctor2ShiftRef { get; set; }
}
