using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class ObstetricRecord
{
    public int RecordId { get; set; }

    public int? Gravida { get; set; }

    public int? Para { get; set; }

    public DateOnly? Lmpdate { get; set; }

    public int? GestationalAgeWeeks { get; set; }

    public int? FetalHeartRateBpm { get; set; }

    public DateOnly? ExpectedDueDate { get; set; }

    public string? ComplicationsNotes { get; set; }

    public virtual MedicalRecord Record { get; set; } = null!;
}
