using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class PrescriptionDetail
{
    public int PrescriptionDetailId { get; set; }

    public int PrescriptionId { get; set; }

    public string Dosage { get; set; } = null!;

    public string Duration { get; set; } = null!;

    public string? Instruction { get; set; }

    public int MedicineVersionId { get; set; }

    public virtual MedicineVersion MedicineVersion { get; set; } = null!;

    public virtual Prescription Prescription { get; set; } = null!;
}
