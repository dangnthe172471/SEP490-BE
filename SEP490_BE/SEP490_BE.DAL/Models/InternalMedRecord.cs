using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class InternalMedRecord
{
    public int RecordId { get; set; }

    public int? BloodPressure { get; set; }

    public int? HeartRate { get; set; }

    public decimal? BloodSugar { get; set; }

    public string? Notes { get; set; }

    public virtual MedicalRecord Record { get; set; } = null!;
}
