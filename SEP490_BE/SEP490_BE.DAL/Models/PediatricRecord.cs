using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class PediatricRecord
{
    public int RecordId { get; set; }

    public decimal? WeightKg { get; set; }

    public decimal? HeightCm { get; set; }

    public int? HeartRate { get; set; }

    public decimal? TemperatureC { get; set; }

    public virtual MedicalRecord Record { get; set; } = null!;
}
