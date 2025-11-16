using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class MedicalService
{
    public int MedicalServiceId { get; set; }

    public int RecordId { get; set; }

    public int ServiceId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual MedicalRecord Record { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
