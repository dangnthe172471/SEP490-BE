using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? Category { get; set; }

    public int? TestTypeId { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<MedicalService> MedicalServices { get; set; } = new List<MedicalService>();

    public virtual TestType? TestType { get; set; }
}
