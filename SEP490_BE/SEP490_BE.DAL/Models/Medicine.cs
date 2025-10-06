using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class Medicine
{
    public int MedicineId { get; set; }

    public int ProviderId { get; set; }

    public string MedicineName { get; set; } = null!;

    public string? SideEffects { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();

    public virtual PharmacyProvider Provider { get; set; } = null!;
}
